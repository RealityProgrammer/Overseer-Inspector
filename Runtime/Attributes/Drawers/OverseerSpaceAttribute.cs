public class OverseerSpaceAttribute : AdditionDrawerAttribute
{
    public float Amount { get; private set; }
    public OverseerSpaceAttribute(float space) {
        Amount = space;
    }

    public override string ToString() {
        return "OverseerSpaceAttribute(" + Amount + ")";
    }
}
