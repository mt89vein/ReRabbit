namespace ReRabbit.Abstractions.Settings
{
    /// <summary>
    /// ����� �� �������� ������������� �������� (��������) ����� ������������.
    /// </summary>
    public enum RetryPolicyType
    {
        /// <summary>
        /// ��� ��������.
        /// </summary>
        Zero = 0,

        /// <summary>
        /// ����������� �����.
        /// </summary>
        Constant = 1,

        /// <summary>
        /// �������� �����.
        /// </summary>
        Linear = 2,

        /// <summary>
        /// ���������������� �����.
        /// </summary>
        Exponential = 3
    }
}