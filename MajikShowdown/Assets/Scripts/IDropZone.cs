public interface IDropZone
{
    bool CanReceive(DraggableNode node);
    void Receive(DraggableNode node);
    void Release(DraggableNode node);
}