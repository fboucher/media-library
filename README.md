# Media Library

A self-hosted web app for browsing, describing, and running AI object detection on your images and videos.

> **AI service**: any OpenAI-compatible API (local or cloud) — bring your own endpoint, key, and model.

## Run

```bash
docker compose up
```

or if you have .NET 10 installed:

```bash
dotnet run --project src/MediaLibrary
```

Open [http://localhost:8080](http://localhost:8080).
Media files and the database are persisted in a named volume (`media-data`) so they survive container restarts.

## How it works

See [docs/how-it-works.md](docs/how-it-works.md) for a full walkthrough of the app.

## Example / Tutorial

Coming up soon...

## Technologies

- .NET 10 / Blazor Server
- MudBlazor
- SQLite + Dapper
- FFmpeg (video thumbnails)
- Podman / Docker

## Contributing

1. Fork the repo and create a feature branch.
2. Keep changes focused — one concern per PR.
3. Open a pull request with a short description of what and why.

Issues and feedback are welcome.

## License

[MIT](LICENSE)
