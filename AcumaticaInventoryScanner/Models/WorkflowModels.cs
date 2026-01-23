/*
 * Created by: AcuPower LTD
 * Website: acupowererp.com
 * Purpose: Workflow models for scanning-based operations (counts, adjustments, receiving, picking)
 */

using SQLite;

namespace AcuPower.AcumaticaInventoryScanner.Models;

public enum CountSessionType
{
    Physical = 0,
    Cycle = 1
}

public enum DocumentDraftType
{
    InventoryAdjustment = 0,
    ReceivingPutAway = 1,
    PickingPacking = 2
}

public class CountSession
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public CountSessionType SessionType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string BinLocation { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public string Notes { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime? NextDueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CountEntry
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string SessionId { get; set; } = string.Empty;
    public string InventoryId { get; set; } = string.Empty;
    public decimal QtyCounted { get; set; }
    public string Unit { get; set; } = "EA";
    public string Warehouse { get; set; } = string.Empty;
    public string BinLocation { get; set; } = string.Empty;
    public string LotSerial { get; set; } = string.Empty;
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
}

public class DocumentDraft
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public DocumentDraftType DraftType { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Warehouse { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DocumentLine
{
    [PrimaryKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string DraftId { get; set; } = string.Empty;
    public string InventoryId { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public string Unit { get; set; } = "EA";
    public string Location { get; set; } = string.Empty;
    public string LotSerial { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
