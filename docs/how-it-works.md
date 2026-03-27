# How it works

## Overview

Media Library is a Blazor Server app. All state lives on the server; the browser receives rendered HTML over a persistent SignalR connection. Media files are stored on disk, metadata in SQLite.

---

## Pages

### Home (`/`)

The main screen has two columns:

- **Left** — connection selector, prompt field, media grid, and an Add Media button.
- **Right** — metadata panel for the selected item.

#### Adding media
Click **Add Media** and pick an image or video from your device. The file is streamed to the server and saved under the configured storage path. For videos, a thumbnail is extracted from the first frame using FFmpeg. The item appears immediately at the top of the grid.

#### Media grid
Thumbnails are shown in a grid. Clicking one opens its details in the metadata panel.

---

### Metadata panel

Displayed on the right side of the home page when an item is selected.

| Section | Description |
|---------|-------------|
| **Preview** | Thumbnail of the selected file. Click it to open a fullscreen lightbox. If detection boxes have been drawn, they carry over into the lightbox. |
| **File info** | File name and size (read-only). |
| **Description** | Editable text field. Click **Save** to persist. Click **Fill with AI** to generate a description using the active AI connection. |
| **Detect** | Type what you want to find (e.g. `eye, ear`) and click **Detect**. Bounding boxes are drawn on the preview in red. Results are saved and listed in the detection history below. |
| **Detection history** | Scrollable list of past detection runs for this item. Click any entry to redraw its boxes. Hover an entry to see the timestamp. |
| **Delete** | Removes the file from disk and its record from the database. Requires confirmation. |

---

### Connections (`/connections`)

Manage AI API connections. Each connection stores:

- **Name** — a label you choose.
- **URL** — base URL of the OpenAI-compatible endpoint (e.g. `http://192.168.1.10:8000/v1`).
- **API key** — sent as a `Bearer` token.
- **Model** — model identifier string.

Use the **Test** button to verify a connection before saving. The active connection is selected from the dropdown on the home page.

---

## AI integration

Both **Fill with AI** and **Detect** call the same OpenAI-compatible `/chat/completions` endpoint.

### Fill with AI
Sends the image (base64 `image_url`) and the prompt from the prompt field. The first paragraph of the response is used as the description.

### Detect
Sends the image with the prompt `Detect: <your text>`. The model is expected to return structured output in the form:

```
<ref>label</ref><bbox>x1,y1,x2,y2</bbox>
```

Multiple bounding boxes for the same label are separated by `;`. Coordinates are percentages (0–100) of the image dimensions. The response is truncated at the first `<sep>` tag to ignore any hallucinated continuation.

Bounding boxes are drawn as an SVG overlay on the image, with coordinate mapping that accounts for `object-fit: contain` letterboxing.

---

## Data storage

| What | Where |
|------|-------|
| Media files | Disk path configured via `Media:StoragePath` (default `/data/media`) |
| Video thumbnails | `{StoragePath}/thumbnails/{id}.jpg` |
| Database | SQLite file at `Database:Path` (default `/data/media-library.db`) |

Tables: `media`, `connections`, `settings`, `detections`.

In the default Podman/Docker setup, everything lives in the `media-data` named volume mounted at `/data`.

---

## Configuration

Key settings in `appsettings.json` (overridable via environment variables):

| Key | Default | Description |
|-----|---------|-------------|
| `Database:Path` | `/data/media-library.db` | SQLite file location |
| `Media:StoragePath` | `/data/media` | Where uploaded files are saved |

---

## Themes

Three built-in themes selectable from the bottom bar: **Light**, **Dark**, and **Tokyo Night**. The choice is persisted in `localStorage`.
