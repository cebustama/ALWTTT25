# Inserta datos.json en la plantilla y escribe la página del wrapped.
# Uso: python wrapped_build.py <plantilla.html> <datos.json> <salida.html>
import sys
tpl, datos, out = sys.argv[1:4]
html = open(tpl, encoding='utf-8').read().replace('/*DATOS*/[]', open(datos, encoding='utf-8').read())
open(out, 'w', encoding='utf-8').write(html)
print(out, len(html) // 1024, 'KB')
