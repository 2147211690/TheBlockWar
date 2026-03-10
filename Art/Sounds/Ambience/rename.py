import os
import re
from mutagen.oggvorbis import OggVorbis

def extract_target_name(title):
    # 提取括号中的内容，如 mus_grass_combat_delta
    match = re.search(r'\((.*?)\)', title)
    return match.group(1) if match else None

def rename_ogg_files():
    for filename in os.listdir('.'):
        if filename.lower().endswith('.ogg'):
            try:
                audio = OggVorbis(filename)
                title = audio.get('title', [None])[0]
                if not title:
                    print(f"⚠️ 跳过（无标题）: {filename}")
                    continue

                new_name = extract_target_name(title)
                if not new_name:
                    print(f"⚠️ 跳过（无括号内容）: {filename} -> 标题: {title}")
                    continue

                new_filename = f"{new_name}.ogg"
                if new_filename == filename:
                    print(f"✅ 已匹配，无需重命名: {filename}")
                    continue
                if os.path.exists(new_filename):
                    print(f"⚠️ 跳过（目标文件已存在）: {filename} -> {new_filename}")
                    continue

                os.rename(filename, new_filename)
                print(f"✅ 重命名: {filename} -> {new_filename}")

            except Exception as e:
                print(f"❌ 错误处理文件 {filename}: {e}")

if __name__ == '__main__':
    rename_ogg_files()