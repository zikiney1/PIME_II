using Godot;
using System;

public partial class CraftingGui : Control
{
    Container ContainerReceitas;
    public override void _Ready(){
        ContainerReceitas = GetNode<Container>("SideBar/SideBarContainer/RecipesContainer");
    }
    public void UpdateItems(){
        RecipeData[] receitas = CraftingSystem.GetRecipes();
        
    }
}
