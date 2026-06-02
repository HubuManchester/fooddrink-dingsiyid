namespace CampusEats.Models;

public enum SaveStatus
{
    Success,
    Inserted,
    Updated,
    Conflict,
    Error
}

public class SaveResult
{
    public bool IsSuccess => Status == SaveStatus.Success || Status == SaveStatus.Inserted || Status == SaveStatus.Updated;
    
    public SaveStatus Status { get; set; }
    
    public int RowsAffected { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public object? ConflictingItem { get; set; }
    
    public static SaveResult InsertSuccess(int rowsAffected) => new()
    {
        Status = SaveStatus.Inserted,
        RowsAffected = rowsAffected,
        Message = "New item inserted successfully"
    };
    
    public static SaveResult UpdateSuccess(int rowsAffected) => new()
    {
        Status = SaveStatus.Updated,
        RowsAffected = rowsAffected,
        Message = "Item updated successfully"
    };
    
    public static SaveResult Conflict(object existingItem, string message) => new()
    {
        Status = SaveStatus.Conflict,
        ConflictingItem = existingItem,
        Message = message
    };
    
    public static SaveResult Error(string message) => new()
    {
        Status = SaveStatus.Error,
        Message = message
    };
}
