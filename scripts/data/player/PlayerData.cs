using System;
using Godot;
using System.Collections.Generic;

namespace Geowar.scripts.data.player;

public class PlayerData
{
    private const string DataDir  = "user://data/";
    private const string DataName = "player.sav";
    private const string DataPath = DataDir + DataName;

    // 数值变化时触发（可选，UI 可以订阅刷新）
    public event Action OnCurrencyChanged;

    // uint 天然非负，无需 Math.Max；如果需要上限再手动限制
    public uint GoldCurrency
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnCurrencyChanged?.Invoke();
        }
    }

    public uint DiamondCurrency
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnCurrencyChanged?.Invoke();
        }
    }

    public uint EmeraldCurrency
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            OnCurrencyChanged?.Invoke();
        }
    }

    public PlayerData()
    {
        // 确保目录存在
        if (!DirAccess.DirExistsAbsolute(DataDir))
        {
            var err = DirAccess.MakeDirRecursiveAbsolute(DataDir);
            if (err != Error.Ok)
                TayTool.Log("存档",$"创建数据目录失败: {err}",TayTool.LogTypeEnum.Err);
        }

        LoadPlayerData();
    }

    // ---------------- 保存 ----------------
    public void SavePlayerData()
    {
        var dict = new Godot.Collections.Dictionary<string, Variant>
        {
            { "Gold",    GoldCurrency },
            { "Diamond", DiamondCurrency },
            { "Emerald", EmeraldCurrency }
        };

        // 用 "using" 保证自动关闭
        using var file = FileAccess.Open(DataPath, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            TayTool.Log("存档",$"保存失败: {FileAccess.GetOpenError()}, 路径={DataPath}",TayTool.LogTypeEnum.Err);
            return;
        }

        file.StoreString(Json.Stringify(dict));
    }

    // ---------------- 读取 ----------------
    private void LoadPlayerData()
    {
        if (!FileAccess.FileExists(DataPath))
        {
            TayTool.Log("存档","存档不存在，使用默认值");
            return;
        }

        using var file = FileAccess.Open(DataPath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            TayTool.Log("存档",$"读取失败: {FileAccess.GetOpenError()}",TayTool.LogTypeEnum.Err);
            return;
        }

        string text = file.GetAsText();
        if (string.IsNullOrWhiteSpace(text))
        {
            TayTool.Log("存档","存档为空",TayTool.LogTypeEnum.Err);
            return;
        }

        var json = new Json();
        if (json.Parse(text) != Error.Ok)
        {
            TayTool.Log("存档",$"解析失败: 行 {json.GetErrorLine()} - {json.GetErrorMessage()}",TayTool.LogTypeEnum.Err);
            return;
        }

        var data = json.Data.AsGodotDictionary();
        GoldCurrency    = data.GetValueOrDefault("Gold",    0).AsUInt32();
        DiamondCurrency = data.GetValueOrDefault("Diamond", 0).AsUInt32();
        EmeraldCurrency = data.GetValueOrDefault("Emerald", 0).AsUInt32();
        
        TayTool.Log("存档",$"读取存档: 金币={GoldCurrency}, 钻石={DiamondCurrency}, 绿宝石={EmeraldCurrency}");
    }

    public void ClearCurrency()
    {
        GoldCurrency = 0;
        DiamondCurrency = 0;
        EmeraldCurrency = 0;
        SavePlayerData();
    }

    // ---------------- 便捷操作 ----------------
    public void AddGold(uint amount)    => GoldCurrency    += amount;
    public void AddDiamond(uint amount) => DiamondCurrency += amount;
    public void AddEmerald(uint amount) => EmeraldCurrency += amount;
    public void SpendGold(uint amount)  => GoldCurrency -= amount;
    public void SpendDiamond(uint amount)  => DiamondCurrency -= amount;
    public void SpendEmerald(uint amount) => EmeraldCurrency -= amount;
}