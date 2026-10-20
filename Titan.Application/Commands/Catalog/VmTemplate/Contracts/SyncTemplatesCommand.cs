namespace Titan.Application.Commands.Catalog.VmTemplate.Contracts;

// Admin trigger (and the one-shot `sync-templates` CLI through TemplateSyncRunner): enqueue every template. No parameters.
public record SyncTemplatesCommand();
