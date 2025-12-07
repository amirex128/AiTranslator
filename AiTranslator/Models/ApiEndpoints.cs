namespace AiTranslator.Models;

public class ApiEndpoints
{
    public string EnglishToPersian { get; set; } = "http://localhost:3001/api/v1/prediction/2d8919bf-3426-4cf2-9a95-11f2539acff6";
    public string PersianToEnglish { get; set; } = "http://localhost:3001/api/v1/prediction/1e86b0ba-1193-42e3-b0ac-d56720689b0f";
    public string GrammarFix { get; set; } = "http://localhost:3001/api/v1/prediction/2195ec5a-7b27-4fbb-8384-7c4765a6ae06";
}

