using UnityEngine;

[CreateAssetMenu(fileName = "New ItemData", menuName = "Naninovel/Item Data")]
public class ItemData : ScriptableObject
{
    [Tooltip("�������� ���� ID (���ϳ뺧 ������� ��ġ ����)")]
    public string itemId;

    [Tooltip("������ �̸� (UI ǥ�ÿ�)")]
    public string itemName;

    [Tooltip("������ ���� (���� ��� ��� ����)")]
    [TextArea]
    public string description;

    [Tooltip("UI�� ǥ�õ� ������ ��������Ʈ")]
    public Sprite iconSprite;

    // �ʿ信 ���� �߰� �Ӽ� ���� ����
    // ��: public AudioClip useSound;
    // ��: public GameObject vfxOnUse;
    // ��: public int sellPrice;
}