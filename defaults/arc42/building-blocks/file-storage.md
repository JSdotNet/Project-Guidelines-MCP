---
title: "Building Block: File Storage"
date: 2026-11-19
status: Accepted
tags: [building-blocks, file-storage, blob-storage, azure, ports-and-adapters, aspire]
---
# Building Block: File Storage

How a module stores files: uploads, generated documents, images. Part of the
[building-block view](../05-building-block-view.md).

## Port and adapter

```csharp
public interface IFileStorage
{
    Task<FileRef> SaveAsync(string container, Stream content, string contentType, CancellationToken ct);
    Task<Stream?> OpenAsync(FileRef file, CancellationToken ct);
    Task DeleteAsync(FileRef file, CancellationToken ct);
}

public sealed record FileRef(string Container, string Name);
```

The port lives in the module; the Azure Blob Storage adapter lives beside the module's data
project, as `Company.Product.{Module}.Data.Blob`, and is hosted locally through Aspire:

```csharp
var blobs = builder.AddAzureStorage("storage").RunAsEmulator().AddBlobs("orders-files");
api.WithReference(blobs);
```

## Rules

- Application code depends only on `IFileStorage`; no SDK type crosses the port.
- File metadata (owner, name, size, content type, `FileRef`) lives in the module's own
  database; the blob holds only the bytes.
- Blob names are generated (a `Guid`), never the uploaded file name.
- Clients download through the Api, or through a short-lived read-only SAS the Api issues
  after its authorization check (ADR 0008); containers are never public.
- The adapter authenticates with managed identity (ADR 0002).
- Upload size and content type are validated at the edge before the stream is read.
