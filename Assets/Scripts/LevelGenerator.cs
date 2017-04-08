using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    public List<GameObject> BlocksToSpawn;
    public BoxCollider AreaToSpawn;

    public Transform PlayerTransform;

    public float MinRandomYPositionOffset = 20;
    public float MaxRandomYPositionOffset = 500;

    public float _currentMaxRandomYPositionOffset;

    public float MinRandomDrag = 1;
    public float MaxRandomDrag = 10;

    public float MoveUpSpeed = 2;

    private Transform _transform;

    public string PlatformTag = "Platform";
    public string PowerupTag = "Powerup";
    public GameObject ExplosionPrefab;

    public int CountOfObjects;
    public int MaxObjects = 1000;

    private Vector3 _startPosition;

    private void Start()
    {
        _transform = transform;
        _startPosition = _transform.position;

        _currentMaxRandomYPositionOffset = MinRandomYPositionOffset;
    }

    public void Reset()
    {
        _moveTimer = 0;
        _transform.position = _startPosition;
    }

    public float SpawnRate = 0.1f;
    private float _spawnTimer;

    public float StartSpawnRate = 100;
    void Update()
    {
        MoveUpwards();

        _spawnTimer += Time.deltaTime;

        _currentMaxRandomYPositionOffset = Mathf.Min(_currentMaxRandomYPositionOffset + Time.deltaTime*StartSpawnRate,
            MaxRandomYPositionOffset);

        if (_spawnTimer < SpawnRate)
            return;
        else if (_spawnTimer >= SpawnRate)
            _spawnTimer = 0;

        if (CountOfObjects < MaxObjects)
        {
            float x = Random.Range(AreaToSpawn.bounds.min.x, AreaToSpawn.bounds.max.x);
            float z = Random.Range(AreaToSpawn.bounds.min.z, AreaToSpawn.bounds.max.z);
            Vector3 newPosition = new Vector3(x, 0, z) + AreaToSpawn.transform.position;

            GameObject g = (GameObject) Instantiate(BlocksToSpawn[Random.Range(0, BlocksToSpawn.Count - 1)]);
            newPosition.y = PlayerTransform.position.y + Random.Range(MinRandomYPositionOffset, _currentMaxRandomYPositionOffset);

            g.transform.position = newPosition;

            g.transform.rotation = Quaternion.Euler(Random.Range(0, 360), 
                Random.Range(0, 360),
                Random.Range(0, 360));
            
            Rigidbody rb = g.GetComponent<Rigidbody>();
            rb.drag = Random.Range(MinRandomDrag, MaxRandomDrag);

            if (g.GetComponent<Collider>() == null)
            {
                MeshCollider mesh = g.AddComponent<MeshCollider>();
                mesh.convex = true;
            }

            CountOfObjects++;

        }
    }

    public float MoveDelay = 1f;
    private float _moveTimer = 0;
    private void MoveUpwards()
    {
        if (_moveTimer >= MoveDelay)
            _transform.Translate(Vector3.up*MoveUpSpeed*Time.deltaTime);
        else
            _moveTimer += Time.deltaTime;
    }

    public void KillPlatform(GameObject g)
    {
        Instantiate(ExplosionPrefab, g.transform.position, Quaternion.identity);
        Destroy(g);
        CountOfObjects--;
    }

    private void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag(PlatformTag) ||
            c.gameObject.CompareTag(PowerupTag))
        {
            KillPlatform((c.gameObject));
        }
    }
}
