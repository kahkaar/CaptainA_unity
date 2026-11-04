# Dev Container

A ready-made Linux environment for editing scripts, formatting them and running the
[fake server](../docs/NEW/FAKE_SERVER.md). The Unity Editor does not run inside it, so scenes,
Play mode and builds still happen on the host machine.

## Setup

1. Install Docker and the VS Code Dev Containers extension.
2. Run **Dev Containers: Reopen in Container**, then choose a configuration:

   | Configuration      | Use it for                                  |
   | ------------------ | ------------------------------------------- |
   | `C# (.NET) on WSL` | Windows with WSL                            |
   | `C# (.NET)`        | Linux, or anywhere without Unity on Windows |

3. Check that the tools are installed:

   ```bash
   csharpier --version && prettier --version && ruff --version && python3 --version
   ```

To check the fake server as well, run [its tests](../docs/NEW/FAKE_SERVER.md#tests).

## Local configurations

A local configuration adds settings for your machine only, such as your own mounts. Any folder here
with `local` in its name is git-ignored, so it is never committed.

1. Copy an existing configuration into a new folder:

   ```bash
   mkdir .devcontainer/local
   cp .devcontainer/devcontainer*.json .devcontainer/local/
   ```

2. In the new `devcontainer.json`, change `name` so that you can tell the two apart, and add your
   mounts. This example also shares Claude Code settings from Windows. Replace `<username>` with
   your username on the Windows host, not your WSL username:

   ```jsonc
   "name": "C# (.NET) on WSL, local mounts",
   "mounts": [
     "source=/mnt/c/Program Files/Unity/Hub/Editor,target=/unity,type=bind",
     "source=/mnt/c/Users/<username>/.claude,target=/home/vscode/.claude,type=bind",
     "source=/mnt/c/Users/<username>/.claude.json,target=/home/vscode/.claude.json,type=bind"
   ],
   ```

   These full files show the same idea for each host:

   <details open>
   <summary><a id="local-wsl"></a>Example for Windows with WSL</summary>

   Replace `<username>` with your username on the Windows host, not your WSL username. Both Claude
   Code paths must already exist there. The full `.devcontainer/local/devcontainer.json` is then:

   ```json
   {
     "name": "C# (.NET) on WSL, local mounts",
     "image": "mcr.microsoft.com/devcontainers/dotnet:2-9.0",
     "features": {
       "ghcr.io/devcontainers/features/node:2": {},
       "ghcr.io/devcontainers/features/python:1": { "toolsToInstall": "ruff" }
     },
     "mounts": [
       "source=/mnt/c/Program Files/Unity/Hub/Editor,target=/unity,type=bind",
       "source=/mnt/c/Users/<username>/.claude,target=/home/vscode/.claude,type=bind",
       "source=/mnt/c/Users/<username>/.claude.json,target=/home/vscode/.claude.json,type=bind"
     ],
     "postCreateCommand": "bash .devcontainer/post-create.sh",
     "postStartCommand": "python3 .devcontainer/wsl/fix-unity-paths.py",
     "customizations": {
       "vscode": {
         "extensions": [
           "esbenp.prettier-vscode",
           "visualstudiotoolsforunity.vstuc",
           "csharpier.csharpier-vscode",
           "charliermarsh.ruff"
         ]
       }
     },
     "remoteUser": "vscode"
   }
   ```

   </details>

   <details>
   <summary><a id="local-linux"></a>Example for Linux machines</summary>

   Copy `.devcontainer/devcontainer*.json` in step 1, because Linux machines do not need the WSL
   configuration. `${localEnv:HOME}` is your home directory on the host, and both mounted paths must
   already exist there. The full `.devcontainer/local/devcontainer.json` is then:

   ```json
   {
     "name": "C# (.NET), local mounts",
     "image": "mcr.microsoft.com/devcontainers/dotnet:2-9.0",
     "features": {
       "ghcr.io/devcontainers/features/node:2": {},
       "ghcr.io/devcontainers/features/python:1": { "toolsToInstall": "ruff" }
     },
     "mounts": [
       "source=${localEnv:HOME}/.claude,target=/home/vscode/.claude,type=bind",
       "source=${localEnv:HOME}/.claude.json,target=/home/vscode/.claude.json,type=bind"
     ],
     "postCreateCommand": "bash .devcontainer/post-create.sh",
     "customizations": {
       "vscode": {
         "extensions": [
           "esbenp.prettier-vscode",
           "visualstudiotoolsforunity.vstuc",
           "csharpier.csharpier-vscode",
           "charliermarsh.ruff"
         ]
       }
     },
     "remoteUser": "vscode"
   }
   ```

   </details>

3. Run **Dev Containers: Reopen in Container** and choose the new name.

Keep the `.devcontainer/post-create.sh` and `.devcontainer/wsl/fix-unity-paths.py` paths as they are,
because a local configuration shares those scripts. It does not receive later changes to the
configuration it was copied from, so copy those across by hand.

## What is in the directory

| File                                               | Purpose                                                           |
| -------------------------------------------------- | ----------------------------------------------------------------- |
| `devcontainer.json`                                | Defines a configuration. The WSL one is in `wsl/`.                |
| `devcontainer-lock.json`                           | Pins the exact feature versions for the configuration next to it. |
| [`post-create.sh`](post-create.sh)                 | Installs CSharpier and Prettier. All configurations share it.     |
| [`wsl/fix-unity-paths.py`](wsl/fix-unity-paths.py) | Makes the Unity paths in `.csproj` files work on WSL.             |

Two files in the repository root also support the container: [`ruff.toml`](../ruff.toml) sets the
Python line length, and [`.gitattributes`](../.gitattributes) keeps `.sh` and `.py` files on LF
line endings, because CRLF breaks them on Linux.

## What gets installed

| Tool                 | Used for            | Comes from                                                |
| -------------------- | ------------------- | --------------------------------------------------------- |
| .NET SDK             | C# tooling          | The `mcr.microsoft.com/devcontainers/dotnet` image        |
| Python 3             | The fake server     | The `python` feature, which uses the Debian package       |
| Ruff                 | Formatting Python   | The `python` feature, through its `toolsToInstall` option |
| Node.js, npm and npx | Running Prettier    | The `node` feature                                        |
| CSharpier            | Formatting C#       | `post-create.sh`                                          |
| Prettier             | Formatting Markdown | `post-create.sh`                                          |

VS Code also installs the extensions listed under `customizations` in each configuration, which
cover Unity, CSharpier, Prettier and Ruff.

## Unity paths on WSL

The C# tooling in VS Code needs the Unity assemblies, but Unity runs on Windows and writes paths such
as `C:\Program Files\Unity\Hub\Editor\...` into the `.csproj` files. Those paths do not exist in the
container, so the WSL configuration handles this in two steps:

1. It mounts `/mnt/c/Program Files/Unity/Hub/Editor` at `/unity`.
2. On every start, `fix-unity-paths.py` rewrites those paths in the `.csproj` files to `/unity/...`.

The `.csproj` files are git-ignored, so the rewritten paths are never committed.

Unity can write the Windows paths back whenever it regenerates these files. To correct them without
restarting the container, run this from the repository root:

```bash
python3 .devcontainer/wsl/fix-unity-paths.py
```

If your Editors are installed somewhere else, change the `source` of the mount. The script only
recognises paths ending in `Program Files\Unity\Hub\Editor`, so a different folder also needs a
change to its `WINDOWS_UNITY_PATH` pattern.

## Changing the setup

Run **Dev Containers: Rebuild Container** after any of these changes, because features and
`post-create.sh` only run when the container is created.

| To change                     | Edit                                                                               |
| ----------------------------- | ---------------------------------------------------------------------------------- |
| CSharpier or Prettier version | The version variables in `post-create.sh`                                          |
| A feature or its version      | Each `devcontainer.json`, and the entry in the `devcontainer-lock.json` next to it |
| Python line length            | `ruff.toml`                                                                        |
| VS Code extensions            | `customizations` in each `devcontainer.json`                                       |

Keep the configurations the same unless a difference is intended, because nothing checks that they
match.

The published feature versions and their digests are listed on the package pages for
[Node.js](https://github.com/devcontainers/features/pkgs/container/features%2Fnode) and
[Python](https://github.com/devcontainers/features/pkgs/container/features%2Fpython).

## Troubleshooting

| Problem                                                           | Likely cause and fix                                                                                        |
| ----------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| `Specify which project file to use` during creation               | `dotnet tool install` ran from the repository root. `post-create.sh` must change directory before it.       |
| `postCreateCommand ... failed`, and `.csproj` paths are not fixed | A failed `postCreateCommand` skips `postStartCommand`. Fix the error in the log, then rebuild.              |
| `No .csproj files found` from `fix-unity-paths.py`                | Unity has not generated them yet.                                                                           |
| C# tooling cannot find Unity types on WSL                         | The `.csproj` files have Windows paths again. Run `fix-unity-paths.py`.                                     |
| `UnityYAMLMerge.exe` fails with `required file not found`         | The mounted Editors are Windows programs and cannot run here. Merge scenes and prefabs from Git on Windows. |
