# Une el inventario (manifest.json) con las etiquetas (etiquetas.csv), escribe catalogo.csv,
# saca las cifras y prepara datos.json (con miniaturas) para el wrapped.
# Uso: python merge.py <carpeta_origen> <manifest.json> <etiquetas.csv> <carpeta_salida> <tmp>
import sys, os, csv, json, base64, io, subprocess, collections
from PIL import Image, ImageOps
root, man, eti, out, tmp = sys.argv[1:6]
ESTADOS = {'incorporada', 'parcial', 'pendiente', 'no-aplica'}
items = {x['i']: x for x in json.load(open(man, encoding='utf-8'))}
rows = {}
with open(eti, encoding='utf-8') as f:
    next(f)
    for line in f:
        p = line.rstrip('\n').split(';')
        if len(p) < 9: continue
        k = next(j for j in range(5, len(p)) if p[j] in ESTADOS)
        rows[int(p[0])] = {'categoria': p[1], 'fuente': p[2], 'descripcion': ', '.join(p[3:k - 1]).replace(', ', ', '),
                           'tema': p[k - 1], 'estado': p[k], 'nota': ' · '.join(x.strip() for x in p[k + 1:-2] if x.strip()),
                           'destacada': int(p[-2]), 'demo': int(p[-1])}
faltan = [i for i in items if i not in rows]
print('filas', len(rows), 'faltan', faltan)
cols = ['i', 'path', 'carpeta', 'tipo_archivo', 'fecha', 'fecha_fuente', 'categoria', 'fuente', 'descripcion', 'tema', 'estado', 'nota', 'destacada', 'demo']
todo = []
for i in sorted(items):
    r = dict(items[i]); r.update(rows.get(i, {'categoria': 'sin-etiquetar'})); todo.append(r)
with open(os.path.join(out, 'catalogo.csv'), 'w', encoding='utf-8', newline='') as f:
    w = csv.writer(f, delimiter=';'); w.writerow(cols)
    for r in todo: w.writerow([r.get(c, '') for c in cols])

C = collections.Counter
def show(t, c): print('\n==', t); [print(f'  {v:4d}  {k}') for k, v in c.most_common()]
show('estado', C(r['estado'] for r in todo))
show('categoria', C(r['categoria'] for r in todo))
show('fuente', C(r['fuente'] for r in todo))
show('mes', C(r['fecha'][:7] for r in todo))
show('carpeta', C(r['carpeta'] for r in todo))
show('tema pendiente', C(r['tema'] for r in todo if r['estado'] == 'pendiente'))
show('tema parcial', C(r['tema'] for r in todo if r['estado'] == 'parcial'))
show('estado x categoria', C((r['categoria'], r['estado']) for r in todo))
show('anio x estado', C((r['fecha'][:4], r['estado']) for r in todo))
print('\ndestacadas', sum(r['destacada'] for r in todo), 'demo', sum(r['demo'] for r in todo))
show('demo por estado', C(r['estado'] for r in todo if r['demo']))

def thumb(r):
    src = os.path.join(root, r['path'])
    if r['tipo_archivo'] == 'video':
        f = os.path.join(tmp, f"vf_{r['i']:03d}.jpg")
        if not os.path.exists(f):
            dur = r.get('segundos', 0) or 0
            subprocess.run(['ffmpeg', '-y', '-v', 'error', '-ss', str(dur * 0.4), '-i', src, '-frames:v', '1', '-vf', 'scale=640:-2', f])
        src = f
    im = ImageOps.exif_transpose(Image.open(src)).convert('RGB'); im.thumbnail((320, 320))
    b = io.BytesIO(); im.save(b, 'JPEG', quality=66)
    return 'data:image/jpeg;base64,' + base64.b64encode(b.getvalue()).decode()

datos = [{k: r.get(k) for k in ['i', 'path', 'carpeta', 'tipo_archivo', 'fecha', 'categoria', 'fuente', 'descripcion', 'tema', 'estado', 'nota', 'destacada', 'demo']} | {'img': thumb(r)} for r in todo]
json.dump(datos, open(os.path.join(tmp, 'datos.json'), 'w', encoding='utf-8'), ensure_ascii=False)
print('datos.json', os.path.getsize(os.path.join(tmp, 'datos.json')) // 1024, 'KB')
