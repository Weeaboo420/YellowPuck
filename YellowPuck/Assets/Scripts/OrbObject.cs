using UnityEngine;

public class OrbObject : MonoBehaviour
{
    private LevelManager _levelManager;
    private Animator _animator;

    public LevelManager LevelManager
    {
        set
        {
            _levelManager = value;
        }
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.Play("OrbFlash", 0, Random.Range(0f, 1f)); //Random animation start offset
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            _levelManager.AddScore();
            Destroy(this.gameObject);
        }
    }
}
