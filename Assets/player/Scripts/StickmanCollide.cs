using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickmanCollide : MonoBehaviour
{
    [SerializeField] List<GameObject> accessorys;
    SkinnedMeshRenderer renderer;
    MeshCollider collider;

    void Start()
    {
        collider = GetComponent<MeshCollider>();
        renderer = GetComponent<SkinnedMeshRenderer>();
        StartCoroutine(MeshUpdater());
    }
    private void OnDestroy()
    {
        foreach(GameObject acc in accessorys)
        {
            acc.transform.SetParent(null);

            if(acc.TryGetComponent(out StickmanWeaponCollision weapon))
            weapon.cantDamage = true;

            Rigidbody rb = acc.AddComponent<Rigidbody>();
            rb.AddForce(Vector3.forward * 200);

            acc.AddComponent<AutoDestroy>();
        }

        Destroy(transform.parent.gameObject);
    }

    void UpdateMesh()
    {
        Mesh updatedCollider = new Mesh();
        renderer.BakeMesh(updatedCollider);
        collider.sharedMesh = null;
        collider.sharedMesh = updatedCollider;

    }
    IEnumerator MeshUpdater()
    {
        while (true)
        {
            UpdateMesh();
            yield return new WaitForSeconds(.5f);
        }
    }
}
