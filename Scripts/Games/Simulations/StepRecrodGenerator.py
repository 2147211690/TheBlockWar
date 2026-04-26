#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import os
import re
from pathlib import Path

# ---------- 配置 ----------
METHOD_PREFIX = "Apply"
REQUIRED_PARAM_TYPE = "SimulationContext"
EXCLUDED_TYPES = {"SimulationContext", "IGameState"}

BASE_USINGS = [
    "using System.Text.Json.Serialization;",
    "using TheBlockWar.Scripts.Games.Buffs;",
    "using TheBlockWar.Scripts.Games.GameStates;",
    "using TheBlockWar.Scripts.Games.Regions;",
    "using TheBlockWar.Scripts.Tools;"
]

NAMESPACE = "TheBlockWar.Scripts.Games.Simulations"
OUTPUT_FILE = "Simulator.Steps.cs"
# 只处理匹配此模式的文件
FILE_PATTERN = re.compile(r"Simulator\.Apply.*\.cs$")
# -------------------------

def find_target_cs_files(directory):
    """返回目录下所有匹配 Simulator.Apply*.cs 的文件"""
    files = []
    for entry in os.listdir(directory):
        full = os.path.join(directory, entry)
        if os.path.isfile(full) and FILE_PATTERN.search(entry):
            files.append(full)
    return files

def split_parameters(param_str):
    """
    按逗号分割参数字符串，但忽略尖括号内的逗号（泛型）。
    返回参数片段列表。
    """
    parts = []
    current = []
    angle_depth = 0
    for ch in param_str:
        if ch == ',' and angle_depth == 0:
            parts.append(''.join(current).strip())
            current = []
        else:
            current.append(ch)
            if ch == '<':
                angle_depth += 1
            elif ch == '>':
                angle_depth -= 1
    if current:
        parts.append(''.join(current).strip())
    return parts

def parse_parameter(param):
    """解析单个参数，返回 (type, name) 或 None"""
    param = param.strip()
    if not param:
        return None
    # 匹配类型和名称，支持默认值（忽略）
    # 类型可以是简单标识符或泛型如 List<Vector2I>?
    m = re.match(r'(\w+(?:<[^>]+>)?(?:\?)?)\s+(\w+)(?:\s*=\s*[^,]+)?', param)
    if m:
        return (m.group(1), m.group(2))
    # 降级：按空格分割
    tokens = param.split(maxsplit=1)
    if len(tokens) == 2:
        ptype, rest = tokens
        pname = rest.split('=')[0].strip()
        return (ptype, pname)
    return None

def parse_method_signature(method_text):
    """
    从方法签名文本中提取方法名和参数列表。
    method_text 形如 "public void ApplyCure(Vector2I coords, float cure, SimulationContext context)"
    返回 (method_name, 参数列表)
    """
    # 提取方法名和括号内的内容
    match = re.search(r'\b(\w+)\s*\(([^)]*)\)', method_text)
    if not match:
        return None, None
    method_name = match.group(1)
    param_str = match.group(2)
    # 分割参数
    raw_params = split_parameters(param_str)
    params = []
    for p in raw_params:
        parsed = parse_parameter(p)
        if parsed:
            params.append({'type': parsed[0], 'name': parsed[1]})
    return method_name, params

def extract_methods_from_file(filepath):
    """从单个 .cs 文件中提取所有符合条件的 Apply 方法"""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # 匹配方法定义（public 开头，包含方法体的大括号先不管，只提取签名）
    # 正则：public ... 返回类型 方法名(参数列表)
    # 使用非贪婪跨行匹配到第一个 { 之前
    pattern = r'public\s+(?:static\s+)?(?:partial\s+)?(?:async\s+)?(\w+(?:<[^>]+>)?)\s+(\w+)\s*\(([^)]*)\)\s*(?:\{|;)'
    methods = []
    for match in re.finditer(pattern, content, re.DOTALL):
        return_type = match.group(1)   # 未使用
        method_name = match.group(2)
        param_str = match.group(3)

        if not method_name.startswith(METHOD_PREFIX):
            continue

        # 解析参数
        raw_params = split_parameters(param_str)
        params = []
        for p in raw_params:
            parsed = parse_parameter(p)
            if parsed:
                params.append({'type': parsed[0], 'name': parsed[1]})

        # 检查是否包含 REQUIRED_PARAM_TYPE
        if not any(p['type'] == REQUIRED_PARAM_TYPE for p in params):
            continue

        # 过滤掉第一个 IGameState 和所有 SimulationContext
        filtered = []
        for i, p in enumerate(params):
            if p['type'] in EXCLUDED_TYPES:
                if p['type'] == 'IGameState' and i == 0:
                    continue
                if p['type'] == REQUIRED_PARAM_TYPE:
                    continue
            filtered.append(p)

        methods.append({
            'method_name': method_name,
            'step_name': method_name + 'Step',
            'parameters': filtered
        })
    return methods

def to_pascal_case(name):
    if not name:
        return name
    return name[0].upper() + name[1:]

def generate_step_record(method):
    """生成单个 record 的代码字符串"""
    ctor_args = []
    call_args = []
    for p in method['parameters']:
        pascal = to_pascal_case(p['name'])
        ctor_args.append(f"{p['type']} {pascal}")
        call_args.append(pascal)
    call_args.append("context")
    step_name = method['step_name']
    method_name = method['method_name']
    record = f"""public record {step_name}({', '.join(ctor_args)}) : ApplyStep
{{
    public override void Apply(IGameState state, SimulationContext context)
    {{
        state.{method_name}({', '.join(call_args)});
    }}
}}
"""
    return record

def generate_output_code(methods):
    """生成完整的 Simulator.Steps.cs 内容"""
    lines = []
    lines.extend(BASE_USINGS)
    lines.append("")
    lines.append(f"namespace {NAMESPACE};")
    lines.append("")
    for m in methods:
        lines.append(f"[JsonDerivedType(typeof({m['step_name']}), nameof({m['step_name']}))]")
    lines.append("public abstract record ApplyStep")
    lines.append("{")
    lines.append("    public abstract void Apply(IGameState state, SimulationContext context);")
    lines.append("}")
    lines.append("")
    for m in methods:
        lines.append(generate_step_record(m))
    return "\n".join(lines)

def main():
    script_dir = Path(__file__).parent.absolute()
    os.chdir(script_dir)   # 确保工作目录是脚本所在目录

    print(f"Working directory: {script_dir}")
    cs_files = find_target_cs_files(script_dir)
    if not cs_files:
        print("No files matching 'Simulator.Apply*.cs' found.")
        return

    all_methods = []
    for filepath in cs_files:
        print(f"Processing {os.path.basename(filepath)}...")
        methods = extract_methods_from_file(filepath)
        all_methods.extend(methods)

    if not all_methods:
        print("No applicable methods (Apply* with SimulationContext) found.")
        return

    # 按 step_name 去重
    seen = set()
    unique = []
    for m in all_methods:
        key = m['step_name']
        if key not in seen:
            seen.add(key)
            unique.append(m)

    print(f"Found {len(unique)} unique step types: {', '.join(m['step_name'] for m in unique)}")

    output_path = script_dir / OUTPUT_FILE
    code = generate_output_code(unique)
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(code)

    print(f"✅ Successfully generated {output_path}")

if __name__ == '__main__':
    main()