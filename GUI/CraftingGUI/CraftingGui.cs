using Godot;
using System;

public partial class CraftingGui : HBoxContainer
{
    ItemList RecipeList;
    RecipeData[] recipes;
    RichTextLabel descriptionText;
    HBoxContainer IngridientsContainer;
    Control IngridientIcon;
    [Export] PackedScene IngridientIconScene;

    public override void _Ready(){
        RecipeList = GetNode<ItemList>("SideBar/SideBarContainer/RecipesContainer");
        descriptionText = GetNode<RichTextLabel>("CraftingWindow/RecipeContainer/RecipeLayout/Description/RichTextLabel");
        IngridientsContainer = GetNode<HBoxContainer>("CraftingWindow/RecipeContainer/RecipeLayout/Ingridients");
        RecipeList.ItemSelected += OnSelectRecipe;
        
        IngridientIcon = IngridientIconScene.Instantiate() as Control;
    }
    public void Activate(){
        Visible = true;
        UpdateRecipes();
        if(RecipeList.ItemCount == 0) return;
        RecipeList.Select(0);
    }
    public void Deactivate(){
        Visible = false;
        recipes = null;
    }
    public void UpdateRecipes(){
        recipes = CraftingSystem.GetRecipes();
        RecipeList.Clear();
        foreach (RecipeData recipe in recipes){
            // if(recipe.known) continue;

            ItemResource result = recipe.result;
            try{
                RecipeList.AddItem(result.name,result.icon);
            }catch(Exception e){
                GD.Print(e);
            }
        }
    }

    public void OnSelectRecipe(long index){
        RecipeData recipe = recipes[index];
        
        descriptionText.Text = recipe.result.description;

        NodeMisc.RemoveAllChildren(IngridientsContainer);
        foreach (Ingredient ingredient in recipe.ingredients){
            Control newIngridientIcon = IngridientIcon.Duplicate() as Control;
            newIngridientIcon.GetNode<TextureRect>("Icon").Texture = ingredient.GetItem().icon;
            newIngridientIcon.GetNode<RichTextLabel>("Text").Text = ingredient.quantity.ToString();

            IngridientsContainer.AddChild(newIngridientIcon);
        }

    }

}
