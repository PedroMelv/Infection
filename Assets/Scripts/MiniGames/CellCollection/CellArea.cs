using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CellArea : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField] private TextMeshProUGUI collectedText;
    [SerializeField] private GameObject enemyCell;
    [SerializeField] private GameObject enemySpinCell;

    [SerializeField] private GameObject collectCell;

    private List<GameObject> collectCells = new List<GameObject>();
    private List<GameObject> enemyCells = new List<GameObject>();

    private int currentLevel = 0;

    private bool generated;

    private Coroutine Generation;

    private int collectedCells = 0;

    private float timer = 3;

    public static CellArea i;

    private void Awake()
    {
        i = this;
    }

    private void Start()
    {
        generated = true;
        InitializeCell();
        collectedText.SetText(
            "Fase " + currentLevel + "/5");
    }

    private void Update()
    {
        if(collectCells.Count <= 0 && generated)
        {
            currentLevel++;
            if(currentLevel > 5)
            {
                Debug.Log("Minigame acabou");
                return;
            }
            collectedText.SetText(
            "Fase " + currentLevel + "/5");
            RestartCell();
        }
    }

    public void InitializeCell()
    {
        collectedCells = 0;
        currentLevel = 1;
        RestartCell();
    }
    public void RestartCell()
    {
        if (Generation != null)
        {
            StopCoroutine(Generation);
            generated = true;
        }

        timer = 3f;
        Generation = StartCoroutine(ERestartCells());
    }
    private IEnumerator ERestartCells()
    {
        if (!generated) yield break;

        generated = false;

        for (int i = 0; i < enemyCells.Count; i++)
        {
            Destroy(enemyCells[i]);
        }
        enemyCells.Clear();

        for (int i = 0; i < collectCells.Count; i++)
        {
            Destroy(collectCells[i]);
        }
        collectCells.Clear();

        Vector3 mouse = Input.mousePosition;
        

        #region Spawn Enemy Cells

        int cellEnemyAmount = 0;
        Vector3 lastCellPos = Vector3.zero;

        while (cellEnemyAmount < 10 * (1 + .5 * (currentLevel - 1)))
        {
            float width = 550f;
            float height = 420f;

            float randomWidth = Random.Range(-(width / 2), (width / 2));
            float randomHeight = Random.Range(-(height / 2), (height / 2));

            Vector3 cellPos = new Vector3(randomWidth, randomHeight, 0);


            if ((Vector3.Distance(lastCellPos, cellPos) > 5f || lastCellPos == Vector3.zero))
            {
                GameObject spawnedCell = (Random.value >= .15f) ? enemyCell : enemySpinCell;

                GameObject cell = Instantiate(spawnedCell, cellPos, Quaternion.identity);

                cell.transform.SetParent(transform, true);
                cell.transform.localPosition = cellPos;
                cell.transform.localScale = Vector3.one;

                enemyCells.Add(cell);

                lastCellPos = cellPos;
                cellEnemyAmount++;
            }

        }

        #endregion

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < enemyCells.Count; i++)
        {
            enemyCells[i].GetComponent<EnemyCell>().activated = true;
        }

        #region Spawn Collect Cells

        int cellCollectAmount = 0;
        lastCellPos = Vector3.zero;

        while (cellCollectAmount < currentLevel)
        {
            float width = 550f;
            float height = 420f;

            float randomWidth = Random.Range(-(width / 2), (width / 2));
            float randomHeight = Random.Range(-(height / 2), (height / 2));

            Vector3 cellPos = new Vector3(randomWidth, randomHeight, 0);

            if ((Vector3.Distance(lastCellPos, cellPos) > 5f || lastCellPos == Vector3.zero))
            {
                GameObject cell = Instantiate(collectCell, cellPos, Quaternion.identity);
                cell.transform.SetParent(transform, true);
                cell.transform.localPosition = cellPos;
                cell.transform.localScale = Vector3.one;

                collectCells.Add(cell);

                lastCellPos = cellPos;
                cellCollectAmount++;
            }

            yield return null;
        }

        #endregion

        generated = true;
    }
    public void RemoveFromCells(GameObject cell)
    {
        collectedCells++;
        collectCells.Remove(cell);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //generated = true;
        //InitializeCell();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log(eventData.fullyExited);
        //generated = true;
        //InitializeCell();
    }
}
