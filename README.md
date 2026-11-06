# Mr.G-Daemon-Windows-Administrator
## GitHub builds

Open **Actions → Windows administrator build → Run workflow** and download
`MrG-Daemon-Administrator-win-x64` from the completed run. Pushes and pull requests
also build. Extract the inner ZIP and run `MrG.Daemon.Admin.UI.exe` from that
folder. The .NET runtime, icon, source commit and SHA-256 checksum are included.

CI builds against the pinned admin-library submodule, checks its offline JSON
and binding contracts, and rejects compilation warnings. MVVM Light has been
replaced with CommunityToolkit.Mvvm in both components. The app connects to the
configured Mr.G daemon when launched; CI does not launch it or control services.
