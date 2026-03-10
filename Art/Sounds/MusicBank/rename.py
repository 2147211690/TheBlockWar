import os, re, json
from mutagen.oggvorbis import OggVorbis

DONE_FILE = '.ogg_rename_done.json'     # 隐藏文件，记录已处理的原文件名

def load_done():
    if os.path.exists(DONE_FILE):
        return set(json.load(open(DONE_FILE, encoding='utf-8')))
    return set()

def save_done(done_set):
    json.dump(list(done_set), open(DONE_FILE, 'w', encoding='utf-8'), indent=2)

def extract_name(title):
    m = re.search(r'\((.*?)\)', title)
    return m.group(1) if m else None

def main():
    done = load_done()
    for old in os.listdir('.'):
        if not old.lower().endswith('.ogg') or old in done:
            continue
        try:
            audio = OggVorbis(old)
            title = audio.get('title', [None])[0]
            new_base = extract_name(title)
            if not new_base:
                continue
            new = f'{new_base}.ogg'
            if new == old:
                done.add(old)          # 名字已符合，视为完成
                continue
            if os.path.exists(new):
                print(f'⚠️ 目标已存在，跳过: {old} -> {new}')
                done.add(old)
                continue
            os.rename(old, new)
            done.add(old)              # 记录“原文件”已处理
            print(f'✅ {old} -> {new}')
        except Exception as e:
            print(f'❌ 错误 {old}: {e}')
    save_done(done)

if __name__ == '__main__':
    main()