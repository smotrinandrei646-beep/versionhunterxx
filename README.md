# VersionHunterX
Поиск приложений и версий на GitHub / GitLab / Codeberg, загрузки с SHA-256, избранное, история.

## Получить APK (без установки чего-либо на ПК)
1. Создайте репозиторий на GitHub и загрузите эту папку (ветка `main`).
2. Вкладка **Actions → Build APK** (запускается сам; или Run workflow).
3. Через ~10 минут скачайте артефакт **VersionHunterX-apk** → внутри `.apk`.

## Локально
`dotnet workload install maui-android` и
`dotnet publish -f net8.0-android -c Release -p:AndroidPackageFormat=apk`
