using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using TelegramSheetBot.Models;

namespace TelegramSheetBot.Services;

public class GoogleSheets
{
    private readonly SheetsService _sheetsService;

    public GoogleSheets(SheetsService sheetsService)
    {
        _sheetsService = sheetsService;
    }

    // public async Task<IEnumerable<SheetRow>> GetList(string spreadsheetId)
    // {
    //     try
    //     {
    //         var request = new SpreadsheetsResource(_sheetsService).Get(spreadsheetId);
    //         request.IncludeGridData = true;
    //
    //         var listData = (await request.ExecuteAsync()).Sheets.First().Data[0].RowData;
    //
    //
    //         List<SheetRow> listRows = new List<SheetRow>();
    //         List<List<SheetRow>> listObject = new List<List<SheetRow>>();
    //
    //
    //         foreach (var row in listData)
    //         {
    //             if (row.Values != null)
    //             {
    //                 foreach (var collection in row.Values)
    //                 {
    //                     if (collection.FormattedValue != null && collection.EffectiveFormat.BackgroundColor != null)
    //                     {
    //                         listRows.Add(new SheetRow()
    //                         {
    //                             Name = collection.FormattedValue ?? "",
    //                          //   ColorBd = collection.EffectiveFormat.BackgroundColor
    //                         });
    //                     }
    //                 }
    //
    //                 if (listRows.Count > 0)
    //                 {
    //                     listObject.Add(listRows);
    //                     listRows = new List<SheetRow>();
    //                 }
    //             }
    //         }
    //
    //    
    //         return listObject.First().Where(i =>
    //             decimal.Parse((i.ColorBd!.Red == null ? "0" : i.ColorBd!.Red.ToString())!) != 1);
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e.Message);
    //         return null!;
    //     }
    // }

/// <summary>
/// получение заголовка в экселе
/// </summary>
/// <param name="spreadsheetId"></param>
/// <returns></returns>
    private async Task<List<SheetRow>> GetHeader(string spreadsheetId)
    {
        try
        {
            var request = new SpreadsheetsResource(_sheetsService).Get(spreadsheetId);
            request.IncludeGridData = true;


            var header = (await request.ExecuteAsync()).Sheets.First().Data[0].RowData[0].Values;


            List<SheetRow> listRows = new List<SheetRow>();

            foreach (var row in header)
            {
                if (row.FormattedValue != null && row.EffectiveFormat.BackgroundColor != null)
                {
                    listRows.Add(new SheetRow()
                    {
                        Name = row.FormattedValue ?? "",
                        ColorBd = row.EffectiveFormat.BackgroundColor
                    });
                }
            }


            return listRows.Where(i =>
                float.Parse((i.ColorBd!.Red == null ? "0" : i.ColorBd!.Red.ToString())!) < 1.0).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return null!;
        }
    }

/// <summary>
/// тестирования подключения
/// </summary>
/// <param name="spreadId"></param>
/// <returns></returns>
    public async Task<bool> TestConnection(string spreadId)
    {
        try
        {
            await new SpreadsheetsResource(_sheetsService).Get(spreadId).ExecuteAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
/// <summary>
/// получение объектов для голосования 
/// </summary>
/// <param name="spreadsheetId"></param>
/// <returns></returns>

    public async Task<List<SheetRow>> ListForPoll(string spreadsheetId)
    {
        var header = await GetHeader(spreadsheetId);

        List<SheetRow> pollList = new List<SheetRow>();

        if (header.Count > 1)
        {
            pollList = header
                .Where(h => float.Parse((h.ColorBd!.Blue == null ? "0" : h.ColorBd!.Blue.ToString())!) > 0.8).ToList();

            foreach (var sheetRow in pollList)
            {
                header.Remove(sheetRow);
            }

            var rnd = new Random();
            int count = 10 - pollList.Count();

            var list = header.OrderBy(_ => rnd.Next()).Take(count).ToList();

            pollList.AddRange(list);
        }


        return pollList;
    }

/// <summary>
/// система бана объектов 
/// </summary>
/// <param name="spreadsheetId"></param>
/// <param name="name"></param>
    public async Task BanSystem(string spreadsheetId, string name)
    {
        try
        {
            var spreadsheet = _sheetsService.Spreadsheets.Get(spreadsheetId);
            spreadsheet.IncludeGridData = true;

            var req = await spreadsheet.ExecuteAsync();
            var sheet = req.Sheets;


            var listCell = sheet.First().Data[0].RowData[0].Values;
            
            foreach (var row in listCell.Select((item, i) => (item, index: i)))
            {
                if (!string.IsNullOrEmpty(row.item.FormattedValue))
                {
                    var color = row.item.EffectiveFormat.BackgroundColor;
                    var blue = float.Parse((color!.Blue == null ? "0" : color.Blue!.ToString())!);
                    var red = float.Parse((color.Red == null ? "0" : color.Red!.ToString())!);
                    var green = float.Parse((color.Green == null ? "0" : color.Green!.ToString())!);
                    
                    //изменения с красного цвета в оранжевый
                    if (blue == 0f && green == 0f && red >= 1f)
                    {
                        var cellFormat = new CellFormat
                        {
                            BackgroundColor = new Color()
                            {
                                Blue = null,
                                Red = 1f,
                                Green = 0.6f
                            }
                        };

                        await UpdateCellFormat(cellFormat, row.index, spreadsheet.SpreadsheetId);
                    }
                    //из оранжевого в зеленный
                    if (blue == 0f && green >= 0.6f && red >= 1f)
                    {
                        var cellFormat = new CellFormat
                        {
                            BackgroundColor = new Color()
                            {
                                Green = 1f
                            }
                        };

                        await UpdateCellFormat(cellFormat, row.index, spreadsheet.SpreadsheetId);
                    }
                    //из светло-синего в зеленый
                    if (blue >= 0.9f && green >= 0.8f && red >= 0.7f)
                    {
                        var cellFormat = new CellFormat
                        {
                            BackgroundColor = new Color()
                            {
                                Green = 1f
                            }
                        };

                        await UpdateCellFormat(cellFormat, row.index, spreadsheet.SpreadsheetId);
                    }
                }
            }
            foreach (var columnBan in listCell.Select((item, i) => (item, index: i)))
            {
                //ban
                if (columnBan.item.FormattedValue == name)
                {
                    var cellFormat = new CellFormat
                    {
                        BackgroundColor = new Color()
                        {
                            Red = 1f
                        }
                    };

                    await UpdateCellFormat(cellFormat, columnBan.index, spreadsheet.SpreadsheetId);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + " BanSystem");
        }
    }

/// <summary>
/// обновление колонки
/// </summary>
/// <param name="cellFormat"></param>
/// <param name="columnIndex"></param>
/// <param name="spreadsheetId"></param>
    private async Task UpdateCellFormat(CellFormat cellFormat, int columnIndex, string spreadsheetId)
    {
        CellData GetFormatted() => new CellData { UserEnteredFormat = cellFormat };

        var requester = new Request
        {
            UpdateCells = new UpdateCellsRequest
            {
                Start = new GridCoordinate
                {
                    SheetId = 0,
                    ColumnIndex = columnIndex,
                    RowIndex = 0,
                },
                Fields = "userEnteredFormat.BackgroundColor",
                Rows = new List<RowData>
                {
                    new RowData
                    {
                        Values = new List<CellData>
                        {
                            GetFormatted()
                        }
                    }
                },
            },
        };

        var requests = new BatchUpdateSpreadsheetRequest
        {
            ResponseIncludeGridData = true,
            Requests = new List<Request> { requester },
        };

        await _sheetsService.Spreadsheets.BatchUpdate(requests, spreadsheetId).ExecuteAsync();
    }
}