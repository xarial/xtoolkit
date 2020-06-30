---
caption: File System Browser
title: File System Browser utilities in xToolkit
description: Utilities to browse files and folder
image: folder-browse.png
---
## Browse Folder

Function browses for a folder allowing to optionally specify the caption of the browse dialog

{% code-snippet { file-name: ~wpf/FileSystemBrowserDocs.*, regions: [browse-folder] } %}

![Browse for folder](folder-browse.png)

## Browse File

### Filters

Use the function below to build the filters mask or use the predefined filters available

![File Filters](file-filter.png)

{% code-snippet { file-name: ~wpf/FileSystemBrowserDocs.*, regions: [browse-file-filter] } %}

### Open

Shows the file open dialog

{% code-snippet { file-name: ~wpf/FileSystemBrowserDocs.*, regions: [browse-file-open] } %}

### Save

Shows the file save dialog

{% code-snippet { file-name: ~wpf/FileSystemBrowserDocs.*, regions: [browse-file-save] } %}