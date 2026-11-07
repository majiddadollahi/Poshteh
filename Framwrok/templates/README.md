# Templates

`Poshteh.Templates/` contains the `dotnet new` template package for applications built on the Poshteh
framework:

| Template | Short name | Identity |
|---|---|---|
| Poshteh Application | `poshteh` | `Poshteh.Templates.Application` |

```powershell
dotnet new install ./Poshteh.Templates/content/PoshtehStarter
dotnet new poshteh -n Mahtab --PoshtehFeed <feed-hosting-Poshteh-packages>
```

See [Poshteh.Templates/README.md](Poshteh.Templates/README.md) for parameters, packaging and maintenance.
