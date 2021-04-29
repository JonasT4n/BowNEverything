using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public abstract class Interactable2D : MonoBehaviour
{
    [InfoBox("This interactable component for 2D used to trigger something or event when touch. " +
        "The Interact Code ID is a special case to trigger something or event and must be unique.")]
    [SerializeField] private string _interactCodeID = "Code ID";
    [SerializeField] private LayerMask _interactWith = ~0;

    private Rigidbody2D _rigidbody;
    private BoxCollider2D _collider;

    public string InteractCodeID => _interactCodeID;

    #region Unity BuiltIn Methods
    // Start is called before the first frame update
    private void Start()
    {
        // Get all components
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();

        // Auto setting to static
        _rigidbody.gravityScale = 0;
        _collider.isTrigger = true;
        _rigidbody.freezeRotation = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_interactWith == (_interactWith | (1 << collision.gameObject.layer)))
            Interact();
    }
    #endregion

    public abstract void Interact();
}
