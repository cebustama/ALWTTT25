# Inventario y hojas de miniaturas de la carpeta de referencias de ALWTTT.
# Uso: python build.py <carpeta_origen> <carpeta_salida>
# No toca los originales: solo los lee. Los vídeos dan un fotograma (ffmpeg) para la hoja.
import os, sys, json, re, subprocess, datetime
from PIL import Image, ImageDraw, ImageOps, ImageFont
Image.MAX_IMAGE_PIXELS = None
root, out = sys.argv[1], sys.argv[2]
IMG = ('.png', '.jpg', '.jpeg', '.webp')
VID = ('.mp4', '.mov', '.webm')
frames = os.path.join(out, 'fotogramas'); os.makedirs(frames, exist_ok=True)

def fecha(nombre, mtime):
    m = re.search(r'(20\d\d)-(\d\d)-(\d\d)', nombre)
    if m: return f'{m[1]}-{m[2]}-{m[3]}', 'nombre'
    m = re.search(r'_(20\d\d)(\d\d)(\d\d)_', nombre)
    if m: return f'{m[1]}-{m[2]}-{m[3]}', 'nombre'
    return datetime.date.fromtimestamp(mtime).isoformat(), 'archivo'

def duracion(p):
    try:
        r = subprocess.run(['ffprobe', '-v', 'error', '-show_entries', 'format=duration', '-of', 'csv=p=0', p], capture_output=True, text=True)
        return float(r.stdout.strip())
    except Exception:
        return 0.0

items = []
for dp, dn, fn in os.walk(root):
    for f in fn:
        p = os.path.join(dp, f); rel = os.path.relpath(p, root).replace(os.sep, '/')
        ext = os.path.splitext(f)[1].lower()
        if ext not in IMG + VID: continue
        st = os.stat(p)
        d, fuente = fecha(f, st.st_mtime)
        it = {'path': rel, 'carpeta': rel.split('/')[0] if '/' in rel else '(raíz)',
              'tipo_archivo': 'video' if ext in VID else 'imagen', 'fecha': d, 'fecha_fuente': fuente,
              'bytes': st.st_size}
        if ext in VID:
            dur = duracion(p); it['segundos'] = round(dur, 1)
            img = os.path.join(frames, f'{len(items):03d}.jpg')
            subprocess.run(['ffmpeg', '-y', '-v', 'error', '-ss', str(max(0.0, dur * 0.4)), '-i', p, '-frames:v', '1', '-vf', 'scale=960:-2', img])
            it['_src'] = img
        else:
            it['_src'] = p
        try:
            with Image.open(it['_src']) as im: it['w'], it['h'] = im.size
        except Exception:
            it['w'] = it['h'] = 0
        items.append(it)

items.sort(key=lambda x: (x['carpeta'] != '(raíz)', x['carpeta'], x['fecha'], x['path']))
T, C, R = 480, 3, 4; N = C * R
try: font = ImageFont.truetype('arialbd.ttf', 22)
except Exception: font = None
for s in range(0, len(items), N):
    chunk = items[s:s + N]; rows = (len(chunk) + C - 1) // C
    sheet = Image.new('RGB', (C * T, rows * T), (40, 40, 40)); d = ImageDraw.Draw(sheet)
    for k, it in enumerate(chunk):
        idx = s + k; it['i'] = idx; it['hoja'] = s // N
        x, y = (k % C) * T, (k // C) * T
        try:
            im = Image.open(it['_src']); im.seek(0)
            im = ImageOps.exif_transpose(im).convert('RGB'); im.thumbnail((T - 8, T - 8))
            sheet.paste(im, (x + (T - im.width) // 2, y + (T - im.height) // 2))
        except Exception:
            d.text((x + 10, y + 200), 'ERR', fill=(255, 0, 0), font=font)
        tag = f'{idx}' + (' VID' if it['tipo_archivo'] == 'video' else '')
        d.rectangle([x, y, x + 90, y + 30], fill=(0, 0, 0)); d.text((x + 5, y + 3), tag, fill=(255, 255, 0), font=font)
    sheet.save(os.path.join(out, f'hoja_{s // N:03d}.jpg'), quality=82)
for it in items: it.pop('_src', None)
json.dump(items, open(os.path.join(out, 'manifest.json'), 'w', encoding='utf-8'), ensure_ascii=False, indent=0)
print('elementos', len(items), 'hojas', (len(items) + N - 1) // N)
