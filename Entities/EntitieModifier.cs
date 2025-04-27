using System;

public class EntitieModifier{

    //quanto ele resiste a dano
    public Half fireResistenceModifier = (Half)0;
    public Half waterResistenceModifier = (Half)0; 
    public Half rockResistenceModifier = (Half)0;

    public float GetFireResistenceModifier() => (float)firePotionResistenceModifier + (float)fireResistenceModifier;
    public float GetWaterResistenceModifier() => (float)waterPotionResistenceModifier + (float)waterResistenceModifier;
    public float GetRockResistenceModifier() => (float)rockPotionResistenceModifier + (float)rockResistenceModifier;

    //quanto ele da dano
    public Half fireDamageModifier = (Half)0;
    public Half waterDamageModifier = (Half)0;
    public Half rockDamageModifier = (Half)0;

    public float GetFireDamageModifier() => (float)firePotionDamageModifier + (float)fireDamageModifier ;
    public float GetWaterDamageModifier() =>(float)waterPotionDamageModifier + (float)waterDamageModifier;
    public float GetRockDamageModifier() => (float)rockPotionDamageModifier + (float)rockDamageModifier;

    
    public Half SpeedModifier = (Half)0;
    public float GetSpeedModifier() => 1f + (float)(SpeedModifier  + SpeedPotionModifier);


    private Half firePotionDamageModifier = (Half)0;
    private Half waterPotionDamageModifier = (Half)0;
    private Half rockPotionDamageModifier = (Half)0;

    private Half firePotionResistenceModifier = (Half)0;
    private Half waterPotionResistenceModifier = (Half)0;
    private Half rockPotionResistenceModifier = (Half)0;

    private Half SpeedPotionModifier = (Half)0;

    public void SetPotionResistenceModifier(Half modifier, ElementsEnum element){
        if(element == ElementsEnum.Fire) firePotionResistenceModifier = modifier;
        else if(element == ElementsEnum.Water) waterPotionResistenceModifier = modifier;
        else if(element == ElementsEnum.Rock) rockPotionResistenceModifier = modifier;
    }

    public void ResetPotionResistenceModifier(ElementsEnum element) {
        if(element == ElementsEnum.Fire) firePotionResistenceModifier = (Half)0;
        else if(element == ElementsEnum.Water) waterPotionResistenceModifier = (Half)0;
        else if(element == ElementsEnum.Rock) rockPotionResistenceModifier = (Half)0;
    }
    public void SetPotionDamageModifier(Half modifier, ElementsEnum element){
        if(element == ElementsEnum.Fire) firePotionDamageModifier = modifier;
        else if(element == ElementsEnum.Water) waterDamageModifier = modifier;
        else if(element == ElementsEnum.Rock) rockDamageModifier = modifier;
    }

    public void ResetPotionDamageModifier(ElementsEnum element) {
        if(element == ElementsEnum.Fire) firePotionDamageModifier = (Half)0;
        else if(element == ElementsEnum.Water) waterDamageModifier = (Half)0;
        else if(element == ElementsEnum.Rock) rockDamageModifier = (Half)0;
    }

    public void SetPotionSpeedModifier(Half modifier) => SpeedPotionModifier = modifier;
    public void ResetPotionSpeedModifier() => SpeedPotionModifier = (Half)0;
}