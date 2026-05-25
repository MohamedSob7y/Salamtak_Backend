using Microsoft.EntityFrameworkCore;
using Salamtak.Domain.Contracts;
using Salamtak.Persistance.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Salamtak.Persistance.DataSeeding
{
    public class DataSeeding : IDataSeeding
    {
        private readonly SalamtakDBContext _salamtakDBContext;
        public DataSeeding(SalamtakDBContext salamtakDBContext)
        {
            _salamtakDBContext = salamtakDBContext;
        }

        public void Intialize()
        {
            //try
            //{
            //    Check if Tables Is Empty Or Not عشان ادخله الداتا => So Need object From DbContext
            //        var hasProduct = _storeDBContext.Products.Any();
            //    var hasBrand = _storeDBContext.productBrands.Any();
            //    var HasTypes = _storeDBContext.productTypes.Any();
            //    if (hasProduct && hasBrand && HasTypes)
            //    {
            //        return;//كدة معناناها فى Data in Tables كدة مش هينفع اعمل Seeding For Data 
            //    }//طب لو مفيش عايز بقا ادخل الداتا دى جوه الTables


            //    //Product has Relation with Brand + Types ومش هينفع ادخل الProduct الاول لازم ادخله Types الاول + Brand عشان يعرف الForign Key تنزله عادى بالتالى مش هعرف ادخله داتا هو الاول لازم ادخل Data For Types+ Brand 
            //    if (!hasBrand)
            //    {
            //        //اقرا الداتا + AddRange
            //        SeedDataFromJson<ProductBrand, int>("brands.json", _storeDBContext.productBrands);
            //    }
            //    if (!HasTypes)
            //    {
            //        //Read Data + AddRange
            //        SeedDataFromJson<ProductType, int>("types.json", _storeDBContext.productTypes);

            //    }
            //    _storeDBContext.SaveChanges();
            //    //محتاج Savehcnages in Database الاول 
            //    if (!hasProduct)
            //    {
            //        SeedDataFromJson<Product, int>("products.json", _storeDBContext.Products);
            //        _storeDBContext.SaveChanges();
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"An Error Accured During Seeding Data {ex}");
            //}
        }
        //private void SeedDataFromJson<T, Tkey>(string filename, DbSet<T> dbset) where T : BaseEntity<Tkey>

        //{
        //    //this Full Path of FIles Brand=> F:\Projects\E_Commerce_System\E_Commerce.Persistance\Data\Json Files\brands.json
        //    //Default Path => Layer اللى بتعمل Run
        //    var filepath = @"..\Salamtak.Persistance\Json Files\" + filename;//.. دى معنانا خرجنى برة Web.api
        //    if (!File.Exists(filepath))
        //    {
        //        throw new FileNotFoundException("Json File not Found", filepath);
        //    }

        //    try
        //    {

        //        var DataStream = File.OpenRead(filepath);
        //        var Data = JsonSerializer.Deserialize<List<T>>(DataStream, new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true,
        //        });
        //        if (Data is not null)
        //        {
        //            dbset.AddRange(Data);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error While Reading Data From Json {ex}");
        //    }
        //}
    }
}
