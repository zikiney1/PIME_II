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

        RecipeList.ItemSelected += (index) => {
            selected = (int)index;
            RecipeList.Select(selected);
            OnSelectRecipe(selected);
        };
        Deactivate();
    }

    /// <summary>
    /// Activates the crafting GUI, updates the list of recipes, and selects the current recipe.
    /// </summary>
    /// <remarks>
    /// This function makes the crafting GUI visible, refreshes the recipes, and selects the currently selected recipe. 
    /// If there are no recipes in the list, it exits early. It also defers focus to the recipe list.
    /// </remarks>
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
    /// <summary>
    /// Updates the recipes in the GUI to be the ones from the crafting system.
    /// </summary>
    /// <remarks>
    /// This function is called whenever the player gains or loses a recipe.
    /// It clears the items in the ItemList and adds all the known recipes to it.
    /// It ignores any recipes that are not known.
    /// </remarks>
    public void UpdateRecipes(){
        recipes = CraftingSystem.GetRecipes();
        RecipeList.Clear();
        foreach (RecipeData recipe in recipes){

            ItemResource result = recipe.result;
            try{
                RecipeList.AddItem(result.name,result.icon);
            }catch(Exception e){
                GD.Print(e);
            }
        }
    }

    /// <summary>
    /// Updates the ingridient display to show the ingredients of the selected recipe.
    /// </summary>
    /// <param name="index">The index of the recipe to show in the list of recipes.</param>
    /// <remarks>
    /// This function is called whenever the selected recipe is changed.
    /// It sets the text of the description field to the description of the recipe.
    /// It clears the ingridient display and adds a TextureRect and RichTextLabel for each ingridient in the recipe.
    /// The RichTextLabel shows the quantity of the ingridient.
    /// It also sets the disabled flag of the craft button to true if the player does not have enough of the ingridients to craft the recipe.
    /// </remarks>
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
            RecipeList.ReleaseFocus();

            
            int direction = (int)InputSystem.GetVector().Y;
            
            if(direction + selected > recipes.Length - 1) selected = 0;
            else if(direction + selected < 0) selected = recipes.Length - 1;
            else selected += direction;
            
            RecipeList.Select(selected);
            OnSelectRecipe(selected);
            if(KeyEvent.IsActionPressed("confirm")) CraftingSystem.CraftItem(player.inventory, recipes[selected]);
        }
    }


}
