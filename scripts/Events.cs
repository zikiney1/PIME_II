using Godot;

public static class Events
{

    public static void Setup()
    {
        //quando o jogo inicia
        EventHandler.AddEvent("OnStart", false, () =>
        {
            Player.Instance.InteractDialog("res://Dialogo/Pai de arana/DialogoInicial/1#_pai_para_arana.txt", "FirstQuest");
            Player.Instance.canAttack = false;
            GameManager.Instance.espirito.dialogPath = "res://Dialogo/fantasma/side_fantasma.txt";
            GameManager.Instance.espirito.EventAtEnd = "side_espirito_start";
            GameManager.Instance.moacir.Visible = true;
            GameManager.Instance.moacir.dialogPath = "res://Dialogo/moacir/side_1_moacir.txt";
            GameManager.Instance.moacir.EventAtEnd = "side_moacir_start";
            
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
                Player.Instance.AddGold(5);
                CraftingSystem.DiscoverRecipe("pocao_jatoba");
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

            GameManager.Instance.Luna.dialogPath = "res://Dialogo/luna/5#_velhaLuna.txt";
            GameManager.Instance.Luna.EventAtEnd = "Quest4#Ended";
        });
        //fala com luna
        EventHandler.AddEvent("Quest4#Ended", false, () =>
        {
            GameManager.Instance.Luna.dialogPath = "res://Dialogo/luna/5_1#_velhaLuna.txt";
            GameManager.Instance.Luna.EventAtEnd = "";
            GameManager.Instance.caua.dialogPath = "res://Dialogo/caua/6#_caua.txt";
            GameManager.Instance.caua.EventAtEnd = "Quest5#Ended";
            CraftingSystem.DiscoverRecipe("pocao_uxi");
            CraftingSystem.DiscoverRecipe("pocao_protetora");
        });
        //fala com caua
        EventHandler.AddEvent("Quest5#Ended", false, () =>
        {
            GameManager.Instance.caua.dialogPath = "res://Dialogo/caua/6_1#_caua.txt";
            GameManager.Instance.caua.EventAtEnd = "";
            GameManager.Instance.thauan.dialogPath = "res://Dialogo/Thauan/7#_thauan.txt";
            GameManager.Instance.thauan.EventAtEnd = "Quest6#Ended";
        });
        //fala com thauan
        EventHandler.AddEvent("Quest6#Ended", false, () =>
        {
            GameManager.Instance.thauan.dialogPath = "";
            GameManager.Instance.thauan.EventAtEnd = "";
            CraftingSystem.DiscoverRecipe("armadura_viva");
        });




        //=========side quests=================
        //=========   moacir  =================

        EventHandler.AddEvent("side_moacir_start", false, () =>
        {
            GameManager.Instance.moacir.dialogPath = "res://Dialogo/moacir/side_2_moacir.txt";
            GameManager.Instance.moacir.EventAtEnd = "side_moacir_end";
        });

        EventHandler.AddEvent("side_moacir_end", false, () =>
        {
            if (GameManager.Instance.moacirQuestEnemies.GetChildCount() == 0)
            {
                Player.Instance.SetDialog("res://Dialogo/moacir/side_3_moacir.txt", "");
                GameManager.Instance.moacir.dialogPath = "";
                GameManager.Instance.moacir.EventAtEnd = "";
                GameManager.Instance.moacir.Visible = false;
                Player.Instance.Add(21);
                Player.Instance.AddGold(25);
            }
            else
            {
                Player.Instance.SetDialog("res://Dialogo/moacir/side_4_moacir.txt", "");
            }
        });
        //=========  espiritos  =================
        EventHandler.AddEvent("side_espirito_start", false, () =>
        {
            GameManager.Instance.espirito.dialogPath = "res://Dialogo/fantasma/side_3_fantasma.txt";
            GameManager.Instance.espirito.EventAtEnd = "side_espiritos_end";
        });
        EventHandler.AddEvent("side_espiritos_end", false, () =>
        {
            if (Player.Instance.inventory.Contains(3))
            {
                Player.Instance.SetDialog("res://Dialogo/fantasma/side_1_fantasma.txt", "");
                GameManager.Instance.espirito.dialogPath = "";
                GameManager.Instance.espirito.EventAtEnd = "";
                CraftingSystem.DiscoverRecipe("pocao_tucum");
                CraftingSystem.DiscoverRecipe("pocao_urucum");
            }
            else
            {
                Player.Instance.SetDialog("res://Dialogo/fantasma/side_2_fantasmas.txt", "");           
            }
        });
        

    }
}