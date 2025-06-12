using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class DialogManager
{
    const string DIALOG_PATH = "res://Dialogs/";
    static Dictionary<string, string[]> dialogDic = new();
    static bool activated = false;
    public static Dictionary<string, Character> Characters = new();

    public static void Setup()
    {
        if (activated) return;
        ReadDialogFile("res://Dialogo/Pai de arana/1#_pai_para_arana.txt");

        ReadDialogFile("res://Dialogo/aponema/2#_apoena.txt");
        ReadDialogFile("res://Dialogo/aponema/2_1#_apoena.txt");

        ReadDialogFile("res://Dialogo/apua/3#_apua.txt");
        ReadDialogFile("res://Dialogo/apua/3_1#_apua.txt");
        ReadDialogFile("res://Dialogo/apua/3_2#_apua.txt");
        ReadDialogFile("res://Dialogo/apua/3_3#_apua.txt");

        ReadDialogFile("res://Dialogo/karai/4#_karai.txt");

        ReadDialogFile("res://Dialogo/luna/5#_velhaLuna.txt");
        ReadDialogFile("res://Dialogo/luna/5_1#_velhaLuna.txt");

        ReadDialogFile("res://Dialogo/caua/6#_caua.txt");

        ReadDialogFile("res://Dialogo/Thauan/7#_thauan.txt");

        ReadDialogFile("res://Dialogo/moacir/side_1_moacir.txt");
        ReadDialogFile("res://Dialogo/moacir/side_2_moacir.txt");
        ReadDialogFile("res://Dialogo/moacir/side_3_moacir.txt");
        ReadDialogFile("res://Dialogo/moacir/side_4_moacir.txt");

        ReadDialogFile("res://Dialogo/fantasma/side_1_fantasma.txt");
        ReadDialogFile("res://Dialogo/fantasma/side_2_fantasmas.txt");
        ReadDialogFile("res://Dialogo/fantasma/side_3_fantasma.txt");
        ReadDialogFile("res://Dialogo/fantasma/side_fantasma.txt");

        ReadDialogFile("res://Dialogo/tutorial/tuto_save_stations.txt");
        SetupCharacters();
        activated = true;
    }
    public static void SetupCharacters()
    {
        Character tutorial = new("", "");
        Character arana = new("Aranã", "res://Dialogo/portrait/arana.png");
        Character PaiDeArana = new("Pai de Arana", "res://Dialogo/portrait/pai_de_arana.png");
        Character apoena = new("Apoena", "res://Dialogo/portrait/apoena.png");
        Character apua = new("Apuã", "res://Dialogo/portrait/apua.png");
        Character karai = new("Karai", "res://Dialogo/portrait/karai.png");
        Character luna = new("Velha Luna", "res://Dialogo/portrait/luna.png");
        Character caua = new("Cauã", "res://Dialogo/portrait/caua.png");
        Character thauan = new("Thauan", "res://Dialogo/portrait/thauan.png");
        Character moacir = new("Moacir", "res://Dialogo/portrait/moacir.png");
        Character espirito = new("Moacir", "res://Dialogo/portrait/espirito.png");

        Characters.Add("tutorial", tutorial);
        Characters.Add("arana", arana);
        Characters.Add("pai_de_arana", PaiDeArana);
        Characters.Add("apoena", apoena);
        Characters.Add("apua", apua);
        Characters.Add("karai", karai);
        Characters.Add("luna", luna);
        Characters.Add("caua", caua);
        Characters.Add("thauan", thauan);
        Characters.Add("moacir", moacir);
        Characters.Add("espirito", espirito);
    }
    public static Character GetCharacter(string name) => Characters[name];
    public static string[] GetDialog(string name) => dialogDic[name];

    static void ReadDialogFile(string path)
    {
        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string[] DialogSequence = file.GetAsText()
                                .Replace("\r", "")
                                .Split('\n')
                                .Where(x => x != "")
                                .ToArray();
        file.Close();

        string name = path.Split('/').Last().Replace(".txt", "");
        dialogDic.Add(name, DialogSequence);
    }
}

public class Character
{
    public string name;
    public Texture2D portrait;
    public Character(string name, string portraitPath)
    {
        this.name = name;
        if(portraitPath == "") return;
        try{
            this.portrait = ResourceLoader.Load<Texture2D>(portraitPath);
        }catch(Exception e){
            GD.PushError(e);
        }
    }
}
