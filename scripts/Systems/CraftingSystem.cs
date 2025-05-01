using System.Collections.Generic;
using System.Linq;
using Godot;

public static class CraftingSystem{
    static Dictionary<string, RecipeData> recipes = new();

    public static void SetupRecipes(){
        const string path = "res://Resources/Recipes/";
        DirAccess dir = DirAccess.Open(path);
        if(dir == null) return;
        dir.ListDirBegin();
        string fileName;
        while((fileName = dir.GetNext()) != ""){
            RecipeResource recipe = GD.Load<RecipeResource>(path + "/" + fileName);
            if(recipe == null) continue;
            if(recipes.ContainsKey(recipe.name)) continue;
            
            recipes.Add(recipe.name, new(recipe));
        }
    }

    public static RecipeData[] GetRecipes(){
        return recipes.Values.ToArray();
    }

    public static RecipeData GetRecipe(string name){
        if(!recipes.ContainsKey(name)) return null;
        return recipes[name];
    }
}

public class RecipeData{
    public string name {get;}
    public string description {get;}
    public Ingredient[] ingredients;
    public bool known = false;

    public RecipeData(RecipeResource recipe){
        name = recipe.name;
        description = recipe.description;
        ingredients = new Ingredient[recipe.ingredients.Length];
        for(int i = 0; i < recipe.ingredients.Length; i++){
            ingredients[i] = new();
            ingredients[i].itemId = recipe.ingredients[i].id;
            ingredients[i].quantity = recipe.quantity[i];
        }
    }
}

public class Ingredient{
    public byte itemId;
    public byte quantity;
    public ItemResource GetItem() => ItemDB.GetItemData(itemId);
}