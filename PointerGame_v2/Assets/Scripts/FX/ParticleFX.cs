using UnityEngine;

public class ParticleFX : MonoBehaviour
{
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private AnimationClip toPlay;

    private GameObject particle;

    private void Start()
    {
        
    }

    public void Play(){
        particle = Instantiate(particlePrefab, transform);

        //adding event to stop playing animation
        // AnimationEvent evt = new AnimationEvent
        // {
        //     time = toPlay.length,
        //     functionName = "StopAnim",
        //     objectReferenceParameter = this
        // };
        
        // toPlay.AddEvent(evt);

        //adding clip to be played
        particle.GetComponent<Animation>().clip = toPlay;
    }

    // public void StopAnim(){
    //     Destroy(particle);
    // }
}
