# Product Requirements Document — Media Library

## Overview

A simple demo web application that displays images and videos from a local directory. Users can upload media one file at a time, manually trigger AI analysis to generate descriptions and tags, and manage saved AI API connections.

---

## Pages

### 1. Main Page

**Layout (top to bottom):**

```
[ Select Connection ▼ ]  [ Edit ]
-----------------------------------------
[ Prompt textarea                        ]
-----------------------------------------
[ Media Grid            | Media Metadata ]
|                       |               |
| [ Add Media ]         |               |
-----------------------------------------
```

#### Connection Bar
- Dropdown listing all saved connections by name.
- Selecting a connection sets it as the active connection for all subsequent AI calls (persisted until the user changes it).
- "Edit" link navigates to the Connections page.
- If no connection is saved, the dropdown is empty and the AI-related controls are disabled.

#### Prompt
- A single editable textarea showing the active prompt.
- Used for both images and videos.
- Changes are saved (persisted in the database).
- Default prompt (used on first run):
  > "Describe this media in 2-3 sentences. Then list relevant tags as a comma-separated list."

#### Media Grid
- Displays all media as thumbnails in a grid.
- Images: display the image directly as thumbnail.
- Videos: display an extracted frame as thumbnail.
- All media loaded at once (no pagination; ~10–20 files expected).
- Clicking a thumbnail selects it and populates the Metadata Panel.
- "Add Media" button sits below the grid.

#### Metadata Panel (right column)
- Always visible; empty when no media is selected.
- Fields (all editable):
  - **File name** (read-only)
  - **File size** (read-only, human-readable e.g. "2.4 MB")
  - **Description** (editable textarea)
  - **Tags** (editable, free-text, comma-separated)
- **"Fill with AI"** button:
  - Disabled if no active connection is selected.
  - On click: sends the media to the AI using the active connection and prompt, then populates Description and Tags fields (user can review and edit before saving).
- **Save** button: persists description and tags to the database.
- **Delete** button: deletes the file from disk and removes its metadata from the database. Requires confirmation.

---

### 2. Connections Page

Accessible via the "Edit" link on the main page.

#### Connection List
- Lists all saved connections by name.
- Each entry has Edit and Delete actions.

#### Add / Edit Connection Form
- **Name** — free text; defaults to the URL if left empty.
- **URL** — free text.
- **API Key** — password field (masked input, never displayed in plain text).
- **Model** — free text (no restrictions).
- **Test** button (optional): sends a minimal request to the API to verify the connection works; shows success or error inline. The connection can be saved without testing.
- Save / Cancel actions.

---

## AI Integration

### Image Analysis
- Uses the OpenAI-compatible Chat Completions API.
- Sends the image as a base64-encoded `image_url` message content block.

### Video Analysis
- Sends the video as a `video_url` message content block (the video file is served by the app and must be reachable from the AI server).
- Example request shape:
```json
{
  "model": "<selected model>",
  "messages": [{
    "role": "user",
    "content": [
      { "type": "video_url", "video_url": { "url": "<video URL>" } },
      { "type": "text", "text": "<active prompt>" }
    ]
  }]
}
```

### Response Parsing
- The app parses the AI response to extract:
  - **Description**: the prose portion of the response.
  - **Tags**: a comma-separated list found in the response.
- Parsed values populate the metadata fields; the user reviews and saves manually.

---

## Media Upload

- One file at a time.
- Supported formats:
  - Images: JPEG, PNG (and other common formats).
  - Videos: MP4, AVI (and other common formats).
- On upload:
  - File is saved to `/data/media` on the server.
  - A database record is created with the file name, file size, and empty description/tags.
  - Video thumbnail is extracted (first frame) and stored for grid display.
- After upload, the new media appears in the grid and is selected, showing its (empty) metadata panel.

---

## Data Storage

### SQLite Database
Tables (logical):

**media**
| column | type | notes |
|---|---|---|
| id | integer PK | |
| file_name | text | original file name |
| file_size | integer | bytes |
| media_type | text | `image` or `video` |
| thumbnail_path | text | path to extracted frame (videos) or null |
| description | text | |
| tags | text | comma-separated |
| created_at | datetime | |

**connections**
| column | type | notes |
|---|---|---|
| id | integer PK | |
| name | text | |
| url | text | |
| api_key | text | stored securely |
| model | text | |

**settings**
| column | type | notes |
|---|---|---|
| key | text PK | |
| value | text | |

Settings keys: `active_connection_id`, `prompt`.

---

## Infrastructure

- **Runtime**: .NET 10 / Blazor Server
- **UI Component Library**: MudBlazor
- **Database**: SQLite
- **Containerization**: Docker
  - Media files stored in volume mounted at `/data/media`
  - `docker-compose.yml` provided for easy setup
- **No authentication** — open access, demo app

---

## Out of Scope (v1)

- Search and filter
- Bulk upload
- User accounts / authentication
- Pagination
