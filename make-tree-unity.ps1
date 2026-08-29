<#
.SYNOPSIS
    Genera dos vistas del repositorio: un arbol legible para humanos y un indice
    curado, listo para el Project Knowledge.

.DESCRIPTION
    NOTA SOBRE CODIFICACION (causa del fallo anterior)
    Windows PowerShell 5.1 asume ANSI (Windows-1252) cuando un .ps1 no lleva
    BOM. Un guion largo guardado en UTF-8 sin BOM se leia como tres bytes
    basura, rompia la cadena que lo contenia y el parser se descarrilaba: de ahi
    los errores en cascada de las lineas 201, 220 y 230.
    Solucion aplicada aqui: el codigo de este script es ASCII PURO. Los
    caracteres no ASCII que aparecen en el fichero de salida se construyen con
    [char]0x...., no se escriben literalmente. Asi el script funciona guardado
    como UTF-8, UTF-8 con BOM o ANSI, indistintamente.

    Salidas:

      tree.txt              Arbol ASCII completo del repo. Para leerlo tu.

      Repo_Tree_Index.md    Indice curado: solo codigo y documentacion, como
                            RUTAS COMPLETAS, una por linea, agrupadas por
                            carpeta. Es lo que entra en el PK (Capa 2).

    Por que dos ficheros y no uno: el arbol ASCII pone el nombre en una linea y
    la carpeta en otra, asi que para saber donde vive un fichero hay que
    reconstruir la jerarquia entera. Un indice de rutas completas responde
    "donde esta X" leyendo una sola linea.

.PARAMETER Root
    Raiz del repo. Por defecto, la carpeta donde esta el script.

.PARAMETER IncludeAssets
    Anade assets de Unity (.asset, .prefab, .unity) al indice curado.

.PARAMETER TreeOnly
    Genera solo tree.txt.

.EXAMPLE
    .\make-tree-unity.ps1
    .\make-tree-unity.ps1 -IncludeAssets
#>

[CmdletBinding()]
param(
    [string]$Root,
    [switch]$IncludeAssets,
    [switch]$TreeOnly
)

$ErrorActionPreference = 'Stop'

if (-not $Root) { $Root = $PSScriptRoot }
if (-not $Root) { $Root = (Get-Location).Path }
$Root = (Resolve-Path -LiteralPath $Root).Path

$treeFile  = Join-Path $Root 'tree.txt'
$indexFile = Join-Path $Root 'Repo_Tree_Index.md'
$stamp     = Get-Date -Format 'yyyy-MM-dd HH:mm'

# Caracteres no ASCII construidos por codigo, nunca escritos literalmente.
# $bt evita ademas el escapado de backticks dentro de cadenas, que era la otra
# fuente de errores de parseo del script anterior.
$bt   = [string][char]0x60      # acento grave (backtick)
$dash = [string][char]0x2014    # guion largo
$aci  = [string][char]0x00ED    # i con tilde
$aco  = [string][char]0x00F3    # o con tilde
$acu  = [string][char]0x00FA    # u con tilde
$ace  = [string][char]0x00E9    # e con tilde
$aca  = [string][char]0x00E1    # a con tilde
$enye = [string][char]0x00F1    # ene con virgulilla

# ------------------------------------------------------------------
#  Exclusiones (compartidas por ambas salidas)
# ------------------------------------------------------------------

$excludedDirs = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
@(
    '.git', '.vs', '.idea', '.vscode',
    'Library', 'Temp', 'Obj', 'obj', 'Logs', 'UserSettings',
    'MemoryCaptures', 'Build', 'Builds', 'Binaries',
    '_Recovery'
) | ForEach-Object { [void]$excludedDirs.Add($_) }

$excludedFilePatterns = @(
    '*.meta', '*.csproj', '*.sln', '*.tmp', '*.user', '*.userprefs',
    '*.pidb', '*.booproj', '*.svd', '*.pdb', '*.mdb', '*.opendb',
    '*.VC.db', '*.unityproj', '*.log', '*.apk', '*.aab',
    'Thumbs.db', '.DS_Store',
    'tree.txt', 'Repo_Tree_Index.md'
)

function Test-ExcludedFile {
    param([System.IO.FileSystemInfo]$Item)
    foreach ($pattern in $script:excludedFilePatterns) {
        if ($Item.Name -like $pattern) { return $true }
    }
    return $false
}

# ------------------------------------------------------------------
#  1. Arbol ASCII completo  ->  tree.txt
# ------------------------------------------------------------------

$treeLines = New-Object 'System.Collections.Generic.List[string]'

function Write-Tree {
    param([string]$Path, [string]$Prefix)

    $items = @(
        Get-ChildItem -LiteralPath $Path -Force -ErrorAction SilentlyContinue |
        Where-Object {
            if ($_.PSIsContainer) { return (-not $script:excludedDirs.Contains($_.Name)) }
            return (-not (Test-ExcludedFile -Item $_))
        } | Sort-Object @{ Expression = { -not $_.PSIsContainer } }, Name
    )

    for ($i = 0; $i -lt $items.Count; $i++) {
        $item   = $items[$i]
        $isLast = ($i -eq ($items.Count - 1))

        if ($isLast) { $branch = '\---' } else { $branch = '+---' }
        $script:treeLines.Add($Prefix + $branch + $item.Name)

        if ($item.PSIsContainer) {
            if ($isLast) { $childPrefix = $Prefix + '    ' } else { $childPrefix = $Prefix + '|   ' }
            Write-Tree -Path $item.FullName -Prefix $childPrefix
        }
    }
}

$rootItem = Get-Item -LiteralPath $Root
$treeLines.Add($rootItem.Name)
$treeLines.Add('(generado ' + $stamp + ')')
Write-Tree -Path $Root -Prefix ''

# tree.txt sin BOM: lo consumen parsers, no PowerShell.
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllLines($treeFile, $treeLines, $utf8NoBom)
Write-Host ('tree.txt             ' + $treeLines.Count + ' lineas')

if ($TreeOnly) { return }

# ------------------------------------------------------------------
#  2. Indice curado  ->  Repo_Tree_Index.md
# ------------------------------------------------------------------

$scopeRoots = @(
    'Assets\Scripts',
    'Assets\Editor',
    'Assets\PinkTrombonePOC',   # POC de voz: lo gobierna SSoT_Singer_Voice, lo toca R6
    'Docs',
    'Packages'
)

# Codigo de terceros: existe en el repo y aparece en tree.txt, pero no es
# codigo de ALWTTT y no se razona sobre el. Fuera del indice curado para que
# no compita en la busqueda con el codigo propio.
$vendorDirs = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
@(
    'MidiPlayer',                    # plugin MIDI de terceros (254 .cs)
    'com.merry-yellow.code-assist'   # paquete de terceros (33 .cs)
) | ForEach-Object { [void]$vendorDirs.Add($_) }

$codeExtensions  = @('.cs', '.shader', '.asmdef', '.compute', '.hlsl', '.cginc')
$docExtensions   = @('.md', '.yaml', '.yml', '.json', '.txt')
$assetExtensions = @('.asset', '.prefab', '.unity')

$wanted = $codeExtensions + $docExtensions
if ($IncludeAssets) {
    $wanted     = $wanted + $assetExtensions
    $scopeRoots = $scopeRoots + 'Assets\Resources'
    $scopeRoots = $scopeRoots + 'Assets\Data'
}

function Get-ScopedFiles {
    param([string]$ScopeRelative)

    $full = Join-Path $script:Root $ScopeRelative
    if (-not (Test-Path -LiteralPath $full)) { return @() }

    Get-ChildItem -LiteralPath $full -Recurse -File -Force -ErrorAction SilentlyContinue |
    Where-Object {
        if (Test-ExcludedFile -Item $_) { return $false }
        if ($script:wanted -notcontains $_.Extension.ToLower()) { return $false }
        $rel = $_.FullName.Substring($script:Root.Length).TrimStart('\', '/')
        foreach ($segment in ($rel -split '[\\/]')) {
            if ($script:excludedDirs.Contains($segment)) { return $false }
            if ($script:vendorDirs.Contains($segment))   { return $false }
        }
        return $true
    }
}

$rootDocs = Get-ChildItem -LiteralPath $Root -File -Force -ErrorAction SilentlyContinue |
            Where-Object { (-not (Test-ExcludedFile -Item $_)) -and ($docExtensions -contains $_.Extension.ToLower()) }

$records = New-Object 'System.Collections.Generic.List[psobject]'

foreach ($doc in $rootDocs) {
    $records.Add([pscustomobject]@{
        Group = '(raiz)'
        Path  = $doc.Name
        Size  = [math]::Round($doc.Length / 1KB, 1)
    })
}

foreach ($scope in $scopeRoots) {
    foreach ($file in (Get-ScopedFiles -ScopeRelative $scope)) {
        $rel = $file.FullName.Substring($Root.Length).TrimStart('\', '/')
        $rel = $rel -replace '\\', '/'
        # Agrupar por la CARPETA que contiene el fichero, truncada a 3 niveles.
        # Tomar los primeros segmentos de la RUTA (incluido el nombre del
        # fichero) hacia grupos falsos de un solo elemento: 'Docs/CURRENT_STATE.md'
        # acababa siendo su propio grupo en vez de caer en 'Docs'.
        $dirParts = @($rel -split '/')
        $dirParts = @($dirParts[0..($dirParts.Count - 2)])   # quitar el nombre del fichero
        if ($dirParts.Count -eq 0) {
            $groupName = '(raiz)'
        } elseif ($dirParts.Count -gt 3) {
            $groupName = ($dirParts[0..2]) -join '/'
        } else {
            $groupName = $dirParts -join '/'
        }
        $records.Add([pscustomobject]@{
            Group = $groupName
            Path  = $rel
            Size  = [math]::Round($file.Length / 1KB, 1)
        })
    }
}

$records = @($records | Sort-Object Path -Unique)

# Nombres duplicados: el PK es plano, asi que dos ficheros homonimos en
# carpetas distintas son indistinguibles una vez adjuntos.
$duplicates = @($records | Group-Object { Split-Path $_.Path -Leaf } | Where-Object { $_.Count -gt 1 })

$out = New-Object 'System.Collections.Generic.List[string]'

$out.Add('# Repo_Tree_Index ' + $dash + ' ' + $aci + 'ndice de rutas del repositorio ALWTTT')
$out.Add('')
$out.Add('**Snapshot: ' + $stamp + '.** Generado por ' + $bt + 'make-tree-unity.ps1' + $bt + '. **No es un documento gobernado** ' + $dash + ' es operativa del PK (Capa 2), igual que ' + $bt + 'PK_Manifest.md' + $bt + '.')
$out.Add('')
$out.Add('**Para qu' + $ace + ' sirve.** El PK es plano: los ficheros adjuntos pierden su carpeta. Este ' + $aci + 'ndice devuelve la ruta real de cualquier fichero por su nombre, sin inferirla del ' + $bt + 'namespace' + $bt + '. Sirve para rellenar la columna *Ruta en repo* de ' + $bt + 'PK_Manifest.md' + $bt + ' y para pedir ficheros por ruta exacta en el File Request Protocol.')
$out.Add('')
$out.Add('**Caducidad.** Es una foto. Regenerar cuando un lote cree, borre o mueva ficheros, y anotar la regeneraci' + $aco + 'n en ' + $bt + 'PK_Manifest.md' + $bt + ' secci' + $aco + 'n C. Un ' + $aci + 'ndice de rutas viejo hace pedir ficheros que ya no existen.')
$out.Add('')
$out.Add('**Alcance.** C' + $aco + 'digo y documentaci' + $aco + 'n: ' + $bt + ($scopeRoots -join ($bt + ', ' + $bt)) + $bt + ', m' + $aca + 's los documentos sueltos de la ra' + $aci + 'z. Extensiones: ' + $bt + ($wanted -join ($bt + ' ' + $bt)) + $bt + '.')
$out.Add('')
$out.Add('**Fuera: c' + $aco + 'digo de terceros** (' + $bt + ($vendorDirs -join ($bt + ', ' + $bt)) + $bt + '). Existe en el repo y sale en ' + $bt + 'tree.txt' + $bt + ', pero no es c' + $aco + 'digo de ALWTTT; indexarlo solo har' + $aci + 'a que compita en la b' + $acu + 'squeda con el c' + $aco + 'digo propio.')
if (-not $IncludeAssets) {
    $out.Add('')
    $out.Add('Assets de Unity (' + $bt + '.asset' + $bt + ' / ' + $bt + '.prefab' + $bt + ' / ' + $bt + '.unity' + $bt + ') **fuera**; regenerar con ' + $bt + '-IncludeAssets' + $bt + ' si un lote de contenido los necesita.')
}
$out.Add('')
$out.Add('**Total indexado: ' + $records.Count + ' ficheros.**')
$out.Add('')

$out.Add('| Carpeta | Ficheros | KB |')
$out.Add('|---|---:|---:|')
foreach ($group in ($records | Group-Object Group | Sort-Object Name)) {
    $sum = ($group.Group | Measure-Object -Property Size -Sum).Sum
    if ($null -eq $sum) { $sum = 0 }
    $kb = [math]::Round($sum, 0)
    $out.Add('| ' + $bt + $group.Name + $bt + ' | ' + $group.Count + ' | ' + $kb + ' |')
}
$out.Add('')

if ($duplicates.Count -gt 0) {
    $out.Add('## Nombres duplicados (atenci' + $aco + 'n)')
    $out.Add('')
    $out.Add('El PK es plano: estos nombres no identifican un fichero de forma un' + $aci + 'voca. Al pedirlos, usar la ruta completa.')
    $out.Add('')
    foreach ($dup in $duplicates) {
        $paths = ($dup.Group | ForEach-Object { $bt + $_.Path + $bt }) -join ' / '
        $out.Add('- **' + $dup.Name + '** ' + $dash + ' ' + $paths)
    }
    $out.Add('')
}

$out.Add('## Rutas')
$out.Add('')
$fence = '```'
foreach ($group in ($records | Group-Object Group | Sort-Object Name)) {
    $out.Add('### ' + $group.Name)
    $out.Add('')
    $out.Add($fence)
    foreach ($record in ($group.Group | Sort-Object Path)) {
        $out.Add($record.Path)
    }
    $out.Add($fence)
    $out.Add('')
}

# El .md se escribe CON BOM para que los acentos sobrevivan a un round-trip
# por el explorador de Windows o por un editor que asuma ANSI.
$utf8Bom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllLines($indexFile, $out, $utf8Bom)

Write-Host ('Repo_Tree_Index.md   ' + $records.Count + ' ficheros indexados')
if ($duplicates.Count -gt 0) {
    Write-Host ('  aviso: ' + $duplicates.Count + ' nombres duplicados') -ForegroundColor Yellow
}
