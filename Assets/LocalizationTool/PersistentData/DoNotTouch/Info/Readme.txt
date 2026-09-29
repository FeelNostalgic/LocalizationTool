Localization Tool
================================================================

A localization workflow for Unity: a windowed manager to create keys,
categories and languages, plus a runtime API that changes language in
your game and fills TextMeshPro text automatically.

QUICK START
----------------------------------------------------------------
1. Tools > LocalizationTool > Manager
2. Tools > LocalizationTool > Install
   (creates the LocalizationToolAPI object in the active scene)
3. Select a TextMeshPro text, then
   Tools > LocalizationTool > UI > Add Addon To TMPro
   and set its Key in the Inspector.

To switch language at runtime, call:

    LocalizationToolAPI.ChangeLanguage("Spanish");

Every addon in the scene updates automatically.

The four tabs
----------------------------------------------------------------
- Dictionary:   keys grouped by category, one value per language,
                plus a rich text editor with live preview.
- Languages:    add, rename, reorder and delete languages. The
                starred one is the default and cannot be deleted.
- Categories:   same for categories. The starred one is the default
                assigned to new keys.
- Configuration: editor preferences, and import / export to CSV,
                JSON and XML.

Exporting to CSV, JSON or XML and importing it back is the way to
send translations to someone else and receive them translated.

Where your data lives
----------------------------------------------------------------
Assets/LocalizationTool/Database/

  LocalizationData.asset   index of keys, languages and categories
  Keys/                    one asset per translation key
  Languages/               one asset per language
  Categories/              one asset per category

Storing one asset per item keeps your translations diffable and
mergeable in version control instead of hidden in one big blob.

Do not hand-edit the files under PersistentData/DoNotTouch; the
tool writes them.

Note: this version runs in the Unity Editor and in Play mode. It
does not compile for a standalone player build. See the project
README for details.


Español
================================================================

Herramienta de localización para Unity: un gestor en ventana para
crear claves, categorías e idiomas, más una API de runtime que
cambia el idioma en tu juego y rellena el texto de TextMeshPro
automáticamente.

INICIO RÁPIDO
----------------------------------------------------------------
1. Tools > LocalizationTool > Manager
2. Tools > LocalizationTool > Install
   (crea el objeto LocalizationToolAPI en la escena activa)
3. Selecciona un texto de TextMeshPro y ve a
   Tools > LocalizationTool > UI > Add Addon To TMPro
   y asigna su Key en el Inspector.

Para cambiar de idioma en runtime, llama a:

    LocalizationToolAPI.ChangeLanguage("Spanish");

Todos los addons de la escena se actualizan automáticamente.

Las cuatro pestañas
----------------------------------------------------------------
- Dictionary:   claves agrupadas por categoría, un valor por idioma,
                y un editor de rich text con vista previa en vivo.
- Languages:    añadir, renombrar, reordenar y eliminar idiomas. El
                marcado con estrella es el predeterminado y no se
                puede eliminar.
- Categories:   igual para categorías. La marcada con estrella es la
                predeterminada y se asigna a las claves nuevas.
- Configuration: preferencias del editor, e importación / exportación
                a CSV, JSON y XML.

Exportar a CSV, JSON o XML y volver a importarlo es la forma de
enviar las traducciones a alguien externo y recibirlas traducidas.

Dónde están tus datos
----------------------------------------------------------------
Assets/LocalizationTool/Database/

  LocalizationData.asset   índice de claves, idiomas y categorías
  Keys/                    un asset por clave de traducción
  Languages/               un asset por idioma
  Categories/              un asset por categoría

Guardar un asset por elemento mantiene tus traducciones
versionables y combinables, en vez de ocultas en un único archivo.

No edites a mano los archivos de PersistentData/DoNotTouch; los
escribe la herramienta.

Nota: esta versión funciona en el Editor y en Play mode. No compila
para una build de player. Los detalles están en el README del
proyecto.
