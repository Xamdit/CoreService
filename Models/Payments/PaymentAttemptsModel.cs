using Service.Entities;
using Service.Framework;

namespace Service.Models.Payments;

public class PaymentAttemptsModel(MyInstance self, MyContext db) : MyModel(self,db)
{
  /**
       * @param array $data
       * @return object|null
       */
  public PaymentAttempt? add(PaymentAttempt data)
  {
    data.CreatedAt = DateTime.Now;
    db.PaymentAttempts.Add(data);
    db.SaveChanges();
    var insert_id = data.Id;
    return get(insert_id, data.PaymentGateway);
  }

  /**
   * @param  int   id
   * @param  string  gateway
   * @return object|null
   */
  public PaymentAttempt? get(int id, string gateway)
  {
    return db.PaymentAttempts.Where(x => x.Id == id && x.PaymentGateway == gateway).FirstOrDefault();
  }

  /**
   * @param  string  $reference
   * @param  string  gateway
   * @return object|null
   */
  public PaymentAttempt? getByReference(string reference, string gateway)
  {
    return db.PaymentAttempts.Where(x => x.Reference == reference && x.PaymentGateway == gateway).FirstOrDefault();
  }

  /**
   * @param int  id
   * @return bool
   */
  public bool delete(int id)
  {
    db.PaymentAttempts.Where(x => x.Id == id).Delete();
    var result = db.SaveChanges();
    return result != 0;
  }
}
