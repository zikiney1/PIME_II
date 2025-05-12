using System.Collections.Generic;
using System.Linq;
using Godot;

public static class CraftingSystem{
    static Dictionary<string, RecipeData> recipes = new();

    /// <summary>
    /// Sets up the crafting system by loading all the recipes in the <c>res://Resources/Recipes/</c> directory.
    /// </summary>
    /// <remarks>
    /// This function is called by the GameManager when it is initialized.
    /// It loads all the recipes in the <c>res://Resources/Recipes/</c> directory and adds them to the recipe database.
    /// If a recipe is already in the database, it is skipped.
    /// If a recipe has a null result, it is skipped.
    /// </remarks>
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
    
    /// <summary>
    /// Gets an array of all recipes in the crafting system.
    /// </summary>
    /// <returns>An array of all recipes in the crafting system.</returns>
    public static RecipeData[] GetRecipes(){
        return recipes.Values.ToArray();
    }

    
    /// <summary>
    /// Makes a recipe known by the crafting system.
    /// </summary>
    /// <param name="name">The name of the recipe to discover.</param>
    /// <returns><c>true</c> if the recipe was successfully discovered, <c>false</c> otherwise.</returns>
    /// <remarks>
    /// This function is useful for when you want to automatically discover recipes in code.
    /// </remarks>
    public static bool DiscoverRecipe(string name){
        if(!recipes.ContainsKey(name)) return false;
        recipes[name].known = true;
        return true;
    }
    
    /// <summary>
    /// Makes all recipes known by the crafting system with the given names.
    /// </summary>
    /// <param name="names">An array of names of the recipes to discover.</param>
    /// <remarks>
    /// This function is useful for when you want to automatically discover multiple recipes in code.
    /// </remarks>
    public static void DiscoverMultiples(string[] names){
        foreach(string name in names){
            if(name.Trim() == "") continue;
            DiscoverRecipe(name);
        }
    }
    
    /// <summary>
    /// Retrieves a recipe by its name from the crafting system.
    /// </summary>
    /// <param name="name">The name of the recipe to retrieve.</param>
    /// <returns>The <c>RecipeData</c> object corresponding to the specified name, or <c>null</c> if the recipe is not found.</returns>
    public static RecipeData GetRecipe(string name){
        if(!recipes.ContainsKey(name)) return null;
        return recipes[name];
    }
    /// <summary>
    /// Crafts an item by removing all the ingredients from the inventory, and adding the result to the inventory.
    /// </summary>
    /// <param name="inventory">The inventory to use for crafting.</param>
    /// <param name="recipe">The recipe to craft.</param>
    /// <remarks>
    /// If the player does not have enough of any of the ingredients, the crafting is cancelled.
    /// </remarks>
    public static void CraftItem(InventorySystem inventory, RecipeData recipe){
        if(!CanCraft(inventory, recipe)) return;
        foreach (Ingredient ingredient in recipe.ingredients){
            inventory.Remove(ingredient.itemId,ingredient.quantity);
        }
        inventory.Add(recipe.result,1);
    }
    
    /// <summary>
    /// Checks if the player has enough items in their inventory to craft a certain recipe.
    /// </summary>
    /// <param name="inventory">The inventory to check.</param>
    /// <param name="recipe">The recipe to check.</param>
    /// <returns><c>true</c> if the player has enough items to craft the recipe, <c>false</c> otherwise.</returns>
    /// <remarks>
    /// This function is useful for checking if the player can craft a recipe before attempting to craft it.
    /// </remarks>
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

    
    /// <summary>
    /// Creates a new RecipeData object from a RecipeResource and a filename.
    /// </summary>
    /// <param name="recipe">The RecipeResource to create the RecipeData from.</param>
    /// <param name="fileName">The filename of the RecipeResource.</param>
    /// <remarks>
    /// The filename is used as the name of the RecipeData.
    /// The ingredients and result are copied from the RecipeResource.
    /// </remarks>
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