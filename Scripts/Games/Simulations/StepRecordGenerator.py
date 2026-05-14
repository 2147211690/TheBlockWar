#!/usr/bin/env python3
"""
读取当前目录下所有 Simulator.Apply*.cs 文件，
解析其中的 Apply* 方法（支持跨行签名），生成对应的 ApplyStep 和 Step 类，
并输出一个自包含的 Simulator.Worker.cs（纯只读，无池化，无 Init/Return）。

所有属性均为 get-only，通过构造函数初始化。
"""

import os
import re

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))

# 匹配 Simulator.Apply*.cs 文件名
FILE_PATTERN = re.compile(r'^Simulator\.Apply\w+\.cs$')

# 匹配方法签名开始：public void ApplyXxx(
SIGNATURE_START = re.compile(r'public\s+void\s+(Apply\w+)\s*\(')
# 匹配 //result: 注释，提取字段组
RESULT_COMMENT = re.compile(r'//\s*result\s*:\s*(.*)')


def parse_method(filepath: str):
    """从文件中解析第一个 Apply* 方法。返回 (method_name, param_list, result_fields)"""
    with open(filepath, 'r', encoding='utf-8') as f:
        raw_text = f.read()

    # 查找方法起始
    match_start = SIGNATURE_START.search(raw_text)
    if not match_start:
        return None

    method_name = match_start.group(1)
    start_index = match_start.end()  # 左括号之后的位置

    # 寻找与左括号匹配的右括号，支持跨行
    depth = 1
    i = start_index
    while i < len(raw_text) and depth > 0:
        if raw_text[i] == '(':
            depth += 1
        elif raw_text[i] == ')':
            depth -= 1
        i += 1
    if depth != 0:
        return None  # 括号不匹配

    param_str = raw_text[start_index:i-1].strip()  # 此时 i-1 指向 ')'

    # 解析参数列表
    params = []
    if param_str:
        def split_params(text: str):
            parts = []
            current = ''
            generic_depth = 0
            for ch in text:
                if ch == '<':
                    generic_depth += 1
                elif ch == '>':
                    generic_depth -= 1
                if ch == ',' and generic_depth == 0:
                    parts.append(current.strip())
                    current = ''
                else:
                    current += ch
            if current.strip():
                parts.append(current.strip())
            return parts

        for part in split_params(param_str):
            tokens = part.split()
            if len(tokens) >= 2:
                name = tokens[-1]
                type_str = ' '.join(tokens[:-1])
                params.append((type_str, name))

    # 查找方法上方的 result 注释
    lines = raw_text.split('\n')
    signature_line = -1
    for i, line in enumerate(lines):
        if SIGNATURE_START.search(line):
            signature_line = i
            break

    result_fields = []
    if signature_line != -1:
        for j in range(signature_line - 1, max(signature_line - 6, -1), -1):
            rm = RESULT_COMMENT.match(lines[j].strip())
            if rm:
                result_str = rm.group(1).strip()
                for field_def in result_str.split():
                    if ':' in field_def:
                        fname, ftype = field_def.split(':', 1)
                        # 强制结果字段参数名首字母小写
                        fname = fname.strip()
                        if fname and fname[0].isupper():
                            fname = fname[0].lower() + fname[1:]
                        result_fields.append((ftype.strip(), fname))
                break

    return method_name, params, result_fields


def generate_step_name(method_name: str):
    """从 ApplyDamage 提取 Damage"""
    if method_name.startswith('Apply'):
        return method_name[5:]
    return method_name


def generate_apply_step_class(method_name: str, params):
    """生成 ApplyXxxStep 类（只读，构造函数注入）"""
    step_name = generate_step_name(method_name)
    class_name = f"Apply{step_name}Step"

    lines = []
    lines.append(f"public class {class_name} : ApplyStep")
    lines.append("{")

    # 只读属性
    for ptype, pname in params:
        prop_name = pname[0].upper() + pname[1:] if pname else pname
        lines.append(f"    public {ptype} {prop_name} {{ get; set; }}")
    lines.append("")

    # 构造函数
    ctor_params = ', '.join(f"{ptype} {pname}" for ptype, pname in params)
    lines.append(f"    public {class_name}({ctor_params})")
    lines.append("    {")
    for _, pname in params:
        prop_name = pname[0].upper() + pname[1:] if pname else pname
        lines.append(f"        {prop_name} = {pname};")
    lines.append("    }")
    lines.append("")

    # Apply 方法
    call_args = ', '.join(
        (pname[0].upper() + pname[1:]) if pname else pname
        for _, pname in params
    )
    lines.append("    public override void Apply(SimObject obj)")
    lines.append("    {")
    lines.append(f"        obj.{method_name}({call_args});")
    lines.append("    }")
    lines.append("}")

    return class_name, '\n'.join(lines)


def generate_step_class(method_name: str, params, result_fields):
    """生成 XxxStep 类（只读，构造函数调用基类并初始化内部 ApplyXxxStep）"""
    step_name = generate_step_name(method_name)
    apply_class = f"Apply{step_name}Step"
    class_name = f"{step_name}Step"

    lines = []
    lines.append(f"public class {class_name} : SimStep")
    lines.append("{")

    # 隐藏基类的 Apply，返回具体类型
    lines.append(f"    public {apply_class} Apply => ({apply_class})base.Apply;")

    # 结果属性（只读）
    for ftype, fname in result_fields:
        prop_name = fname[0].upper() + fname[1:] if fname else fname
        lines.append(f"    public {ftype} {prop_name} {{ get; set; }}")

    if result_fields:
        lines.append("")

    # 构造函数参数列表
    param_list = ["int deep"]
    apply_param_names = []
    for ptype, pname in params:
        apply_name = f"apply{pname[0].upper() + pname[1:]}" if pname else "apply"
        param_list.append(f"{ptype} {apply_name}")
        apply_param_names.append(apply_name)
    for ftype, fname in result_fields:
        param_list.append(f"{ftype} {fname}")

    ctor_params = ', '.join(param_list)

    # 构建传递给 ApplyXxxStep 构造函数的参数列表
    apply_ctor_args = ', '.join(apply_param_names)

    lines.append(f"    public {class_name}({ctor_params})")
    lines.append(f"        : base(new {apply_class}({apply_ctor_args}), deep)")
    lines.append("    {")
    for _, fname in result_fields:
        prop_name = fname[0].upper() + fname[1:] if fname else fname
        lines.append(f"        {prop_name} = {fname};")
    lines.append("    }")
    lines.append("}")

    return class_name, '\n'.join(lines)


def main():
    cs_files = []
    for fname in os.listdir(SCRIPT_DIR):
        if FILE_PATTERN.match(fname):
            cs_files.append(os.path.join(SCRIPT_DIR, fname))

    if not cs_files:
        print("未找到任何 Simulator.Apply*.cs 文件")
        return

    apply_step_types = []
    step_types = []
    apply_classes_code = []
    step_classes_code = []

    for fpath in cs_files:
        result = parse_method(fpath)
        if result is None:
            print(f"警告：未在 {fpath} 中找到有效的 Apply 方法")
            continue
        method_name, params, result_fields = result
        apply_class, apply_code = generate_apply_step_class(method_name, params)
        step_class, step_code = generate_step_class(method_name, params, result_fields)
        apply_step_types.append(apply_class)
        step_types.append(step_class)
        apply_classes_code.append(apply_code)
        step_classes_code.append(step_code)

    output = []
    output.append("using System.Text.Json.Serialization;")
    output.append("using TheBlockWar.Scripts.Games.GameStates;")
    output.append("using TheBlockWar.Scripts.Tools;")
    output.append("")
    output.append("namespace TheBlockWar.Scripts.Games.Simulations;")
    output.append("")

    # ------------------ ApplyStep 抽象基类 ------------------
    for atype in apply_step_types:
        output.append(f'[JsonDerivedType(typeof({atype}), nameof({atype}))]')
    output.append("public abstract class ApplyStep")
    output.append("{")
    output.append("    public abstract void Apply(SimObject obj);")
    output.append("}")
    output.append("")

    # ------------------ SimStep 基类（非泛型，只读）------------------
    for stype in step_types:
        output.append(f'[JsonDerivedType(typeof({stype}), nameof({stype}))]')
    output.append("public class SimStep")
    output.append("{")
    output.append("    public ApplyStep Apply { get; set; }")
    output.append("    public int Deep { get; set; }")
    output.append("    public SimStep(ApplyStep apply, int deep)")
    output.append("    {")
    output.append("        Apply = apply;")
    output.append("        Deep = deep;")
    output.append("    }")
    output.append("}")
    output.append("")

    # ------------------ ApplyXxxStep 类 ------------------
    for code in apply_classes_code:
        output.append(code)
        output.append("")

    # ------------------ XxxStep 类 ------------------
    for code in step_classes_code:
        output.append(code)
        output.append("")

    out_path = os.path.join(SCRIPT_DIR, "Simulator.Worker.cs")
    with open(out_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(output))

    print(f"生成完成：{out_path}")


if __name__ == "__main__":
    main()