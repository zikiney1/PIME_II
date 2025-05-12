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
        string name = "";
        while((fileName = dir.GetNext()) != ""){
            RecipeResource recipe = GD.Load<RecipeResource>(path + "/" + fileName);
            if(recipe == null) continue;
            name = fileName.Replace(".tres","");
            if(recipes.ContainsKey(name)) continue;
            if(recipe.result == null) continue;
            recipes.Add(name, new(recipe,name));
        }
    }

    public static RecipeData[] GetRecipes(){
        return recipes.Values.ToArray();
    }

    public static bool DiscoverRecipe(string name){
        if(!recipes.ContainsKey(name)) return false;
        recipes[name].known = true;
        return true;
    }
    public static void DiscoverMultiples(string[] names){
        foreach(string name in names){
            if(name.Trim() == "") continue;
            DiscoverRecipe(name);
        }
    }

    public static RecipeData GetRecipe(string name){
        if(!recipes.ContainsKey(name)) return null;
        return recipes[name];
    }
    public static void CraftItem(InventorySystem inventory, RecipeData recipe){
        if(!CanCraft(inventory, recipe)) return;
        foreach (Ingredient ingredient in recipe.ingredients){
            inventory.Remove(ingredient.itemId,ingredient.quantity);
        }
        inventory.Add(recipe.result,1);
    }
    public static bool CanCraft(InventorySystem inventory, RecipeData recipe){
        foreach (Ingredient ingredient in recipe.ingredients){
            if(!inventory.Contains(ingredient.itemId,ingredient.quantity)) return false;
        }

        return true;
    }
}

public class RecipeData{
    public string name {get;}
    public Ingredient[] ingredients;
    public bool known = false;
    public ItemResource result;

    public RecipeData(RecipeResource recipe,string fileName){
        name = fileName;
        ingredients = new Ingredient[recipe.ingredients.Length];
        result = ItemDB.GetItemData(recipe.result.id);
        for(int i = 0; i < recipe.ingredients.Length; i++){
            ingredients[i] = new();
            ingredients[i].itemId = recipe.ingredients[i].id;
            ingredients[i].quantity = recipe.quantity[i];
        }
    }
}

public class Ingredient{
    public ItemResource GetItem() => ItemDB.GetItemData(itemId);
    public byte itemId;
    public byte quantity;
}