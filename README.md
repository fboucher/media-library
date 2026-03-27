# Media Library

Simple web application that list images and videos from a directory.
- User can add new media by clicking the "Add Media" button and selecting a file from their device.
- The application supports common image formats (e.g., JPEG, PNG) and video formats (e.g., MP4, AVI).
- The media files are displayed in a grid layout, showing thumbnails.
- Users can click on a thumbnail to view the media details: 
    * file name
    * file size
    * description
    * tags
- media files are stored in a local directory on the server, and their metadata is saved in a SQLite database.

- description and tags are obtained using AI. It's OpenAI compatible API.
- On a different page, users can save connections to the API: URL, API key, and model name. These connections are stored in the database. 
- the active connection can be selected from a dropdown menu when adding media. The application will use the selected connection to obtain the description and tags for the media file.

- The application runs in a docker container, and the media files are stored in a volume that is mounted to the container. This allows the media files to persist even if the container is stopped or removed.
- a docker-compose file is provided to easily set up the application

## Technologies Used

- .NET 10 
- Blazor server
- SQLite
- Docker
