using UnityEngine;


public class CardToHand : MonoBehaviour

{
    public GameObject Hand;
    public GameObject HandCard;


    void Start()
    {
        Hand = GameObject.Find("Hand");
        
        if (Hand != null && HandCard != null)
        {
            GameObject instantiatedHandCard = Instantiate(HandCard, Hand.transform);

            // instantiatedHandCard.transform.SetParent(Hand.transform);
            instantiatedHandCard.transform.localScale = Vector3.one;
            instantiatedHandCard.transform.position = new Vector3(transform.position.x, transform.position.y, -48);
            instantiatedHandCard.transform.eulerAngles = new Vector3(25, 0, 0);      

        }
    }

    private void Update() 
    {

    }

}
