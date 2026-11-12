namespace PM.Identities.EndpointApi.Endpoints;

/// <summary>فیلتر «نوع تأمین‌کننده» در فهرست؛ پیش از ساخت GetVendorListQuery به bool ترجمه می‌شود چون این مفهوم به ماژول Vendor تعلق دارد.</summary>
public enum VendorTypeFilter
{
    Individual = 0,
    Legal = 1
}
