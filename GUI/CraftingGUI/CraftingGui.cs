using Godot;
using System;

public partial class CraftingGui : HBoxContainer
{
    ItemList RecipeList;
    RichTextLabel descriptionText;
    HBoxContainer IngridientsContainer;
    Control IngridientIcon;
    Button craftButton;

    [Export] PackedScene IngridientIconScene;
    Player player;
    RecipeData[] recipes;
    int selected = 0;


    public override void _Ready(){
        RecipeList = GetNode<ItemList>("SideBar/SideBarContainer/RecipesContainer");
        RecipeList.ItemSelected += OnSelectRecipe;

        descriptionText = GetNode<RichTextLabel>("CraftingWindow/RecipeContainer/RecipeLayout/Description/RichTextLabel");
        IngridientsContainer = GetNode<HBoxContainer>("CraftingWindow/RecipeContainer/RecipeLayout/Ingridients");
        craftButton = GetNode<Button>("CraftingWindow/RecipeContainer/RecipeLayout/Actions/CraftButton");
        
        player = GetNode<Player>("../..");
        IngridientIcon = IngridientIconScene.Instantiate() as Control;
    }
    public void Activate(){
        Visible = true;
        UpdateRecipes();
        if(RecipeList.ItemCount == 0) return;
        OnSelectRecipe(selected);

        RecipeList.Select(selected);
        CallDeferred(nameof(DeferredFocus));

    }
    public void DeferredFocus(){
        RecipeList.GrabFocus();
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
        if(CraftingSystem.CanCraft(player.inventory, recipe)){
            craftButton.Disabled = false;
        }
        else{
            craftButton.Disabled = true;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if(!Visible || recipes == null || recipes.Length == 0) return;
        if(@event is InputEventKey KeyEvent){
            
            int direction = (int)InputSystem.GetVector().Y;
            
            if(direction + selected > recipes.Length - 1) selected = recipes.Length - 1;
            else if(direction + selected < 0) selected = 0;
            else selected += direction;
            
            RecipeList.Select(selected);
            OnSelectRecipe(selected);
            if(KeyEvent.IsActionPressed("confirm")) CraftingSystem.CraftItem(player.inventory, recipes[selected]);
        }
    }


}
