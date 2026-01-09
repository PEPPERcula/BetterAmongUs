using AmongUs.Data;

namespace BetterAmongUs.Data.Json;

[Serializable]
internal sealed class OutfitData
{
    public string HatId = HatData.EmptyId;
    public string PetId = PetData.EmptyId;
    public string SkinId = SkinData.EmptyId;
    public string VisorId = VisorData.EmptyId;
    public string NamePlateId = NamePlateData.EmptyId;

    internal static OutfitData GetOutfitData() => BetterDataManager.BetterDataFile.OutfitData.ElementAt(BetterDataManager.BetterDataFile.SelectedOutfitPreset);
    internal static OutfitData GetOutfitData(int index) => BetterDataManager.BetterDataFile.OutfitData.ElementAt(index);

    private static bool ignoreChange;
    internal static void Init()
    {
        FindPreset();

        var Save = () =>
        {
            if (ignoreChange) return;
            GetOutfitData().LoadToData();
            BetterDataManager.BetterDataFile.Save();
        };

        DataManager.Player.Customization.OnHatChanged += Save;
        DataManager.Player.Customization.OnPetChanged += Save;
        DataManager.Player.Customization.OnSkinChanged += Save;
        DataManager.Player.Customization.OnVisorChanged += Save;
        DataManager.Player.Customization.OnNamePlateChanged += Save;
    }

    internal static void FindPreset()
    {
        var collection = BetterDataManager.BetterDataFile.OutfitData;
        int i = 0;
        foreach (var data in collection)
        {
            if (data.HatId == DataManager.Player.Customization.Hat &&
                data.PetId == DataManager.Player.Customization.Pet &&
                data.SkinId == DataManager.Player.Customization.Skin &&
                data.VisorId == DataManager.Player.Customization.Visor &&
                data.NamePlateId == DataManager.Player.Customization.NamePlate)
            {
                BetterDataManager.BetterDataFile.SelectedOutfitPreset = i;
                BetterDataManager.BetterDataFile.Save();
                return;
            }
            i++;
        }

        BetterDataManager.BetterDataFile.SelectedOutfitPreset = 0;
        BetterDataManager.BetterDataFile.Save();
    }

    private void Validate()
    {
        if (!HatManager.Instance.GetUnlockedHats().Any(item => item.ProductId == HatId))
            HatId = HatData.EmptyId;
        if (!HatManager.Instance.GetUnlockedPets().Any(item => item.ProductId == PetId))
            PetId = PetData.EmptyId;
        if (!HatManager.Instance.GetUnlockedSkins().Any(item => item.ProductId == SkinId))
            SkinId = SkinData.EmptyId;
        if (!HatManager.Instance.GetUnlockedVisors().Any(item => item.ProductId == VisorId))
            VisorId = VisorData.EmptyId;
        if (!HatManager.Instance.GetUnlockedNamePlates().Any(item => item.ProductId == NamePlateId))
            NamePlateId = NamePlateData.EmptyId;
    }

    internal void Load(Action callback)
    {
        Validate();

        ignoreChange = true;
        DataManager.Player.Customization.Hat = HatId;
        DataManager.Player.Customization.Pet = PetId;
        DataManager.Player.Customization.Skin = SkinId;
        DataManager.Player.Customization.Visor = VisorId;
        DataManager.Player.Customization.NamePlate = NamePlateId;
        ignoreChange = false;

        callback.Invoke();
        BetterDataManager.BetterDataFile.Save();
    }

    internal void LoadToData()
    {
        HatId = DataManager.Player.Customization.Hat;
        PetId = DataManager.Player.Customization.Pet;
        SkinId = DataManager.Player.Customization.Skin;
        VisorId = DataManager.Player.Customization.Visor;
        NamePlateId = DataManager.Player.Customization.NamePlate;
    }
}
