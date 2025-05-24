using Godot;

public static class Events
{

    public static void Setup()
    {
        EventHandler.AddEvent("OnStart", false, () =>
        {
            Player.Instance.InteractDialog("res://Dialogo/Pai de arana/DialogoInicial/1#_pai_para_arana.txt", "FirstQuest");
            Player.Instance.canAttack = false;
        });
        //apos primeiro dialogo
        EventHandler.AddEvent("FirstQuest", false, () =>
        {
            GameManager.Instance.Apoena.dialogPath = "res://Dialogo/aponema/2#_apoena.txt";
            GameManager.Instance.Apoena.EventAtEnd = "Quest1#Ended";
        });
        //falou com apoena
        EventHandler.AddEvent("Quest1#Ended", false, () =>
        {
            Player.Instance.canAttack = true;
            GameManager.Instance.Apoena.dialogPath = "res://Dialogo/aponema/2_1#_apoena.txt";
            GameManager.Instance.Apoena.EventAtEnd = "";
            GameManager.Instance.Apua.dialogPath = "res://Dialogo/apua/3#_apua.txt";
            GameManager.Instance.Apua.EventAtEnd = "Quest2#Ended";
        });
        //fala com apua
        EventHandler.AddEvent("Quest2#Ended", false, () =>
        {
            GameManager.Instance.Apua.dialogPath = "res://Dialogo/apua/3_1#_apua.txt";
            GameManager.Instance.Apua.EventAtEnd = "vereficaSementes";
            Player.Instance.AddGold(10);
            GameManager.Instance.Karai_dialog.dialogPath = "res://Dialogo/karai/4#_karai.txt";
            GameManager.Instance.Karai_dialog.EventAtEnd = "Quest3#Ended";
        });
        //side quest verefica se tem sementes
        EventHandler.AddEvent("vereficaSementes", false, () =>
        {
            //id da semente
            if (Player.Instance.inventory.Contains(1, 2))
            {
                Player.Instance.SetDialog("res://Dialogo/apua/3_2#_apua.txt", "");
                GameManager.Instance.Apua.dialogPath = "";
                GameManager.Instance.Apua.EventAtEnd = "";
            }
            else
            {
                Player.Instance.SetDialog("res://Dialogo/apua/3_3#_apua.txt", "");
            }
        });
        //fala com karai e compra sementes
        EventHandler.AddEvent("Quest3#Ended", false, () =>
        {
            GameManager.Instance.Karai_dialog.Visible = false;
            GameManager.Instance.Karai_Merchant.Visible = true;
            Player.Instance.RemoveGold(10);
            Player.Instance.Add(1, 2);

            GameManager.Instance.Luna.dialogPath = "res://Dialogo/velhaLuna/5#_velhaLuna.txt";
            GameManager.Instance.Luna.EventAtEnd = "Quest4#Ended";
        });
    }
}