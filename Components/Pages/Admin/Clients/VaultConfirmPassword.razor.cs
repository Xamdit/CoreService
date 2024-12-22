// <copyright file="VaultConfirmPassword.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Service.Core.Engine;

namespace Service.Components.Pages.Admin.Clients;

public class VaultConfirmPasswordRazor : MyComponentBase
{
  [Parameter] public string Name { get; set; }

  public void OnSubmie()
  {
    // form_open(admin_url("clients/vault_encrypt_password"))
  }

  // Show vault entry modal user to re-enter his password
  public void Vault_re_enter_password(int id, object e)
  {
    // var invoker = $(e);
    // var vaultEntry = $('#vaultEntry-' + id);
    // var $confirmPasswordVaultModal =$('#vaultConfirmPassword');
    //
    // appValidateForm($
    // confirmPasswordVaultModal.find('form'),
    // {
    // user_password:
    // 'required'
    // }
    // ,
    // vault_encrypt_password)
    // ;
    //
    // if (!invoker.hasClass('decrypted'))
    // {
    // $
    // confirmPasswordVaultModal.find('form input[name="id"]').val(id);
    // $
    // confirmPasswordVaultModal.modal('show');
    // }
    // else
    // {
    // invoker.removeClass('decrypted');
    // vaultEntry.find('.vault-password-fake').removeClass('hide');
    // vaultEntry.find('.vault-password-encrypted').addClass('hide');
    // }
  }

  // Used to encrypt vault entry password
  public bool Vault_encrypt_password(string form)
  {
    // var vaultEntry = $('#vaultEntry-' +$form.find('input[name="id"]').val());
    // var data = form.serialize();
    // var confirmPasswordVaultModal = ('#vaultConfirmPassword');
    // $.post(form.attr('action'), data).
    // done(function(response)
    // {
    // response = JSON.parse(response);
    // vaultEntry.find('.vault-password-fake').addClass('hide');
    // vaultEntry.find('.vault-view-password').addClass('decrypted');
    // vaultEntry.find('.vault-password-encrypted').removeClass('hide').html(response.password);
    // $
    // confirmPasswordVaultModal.modal('hide');
    // $
    // confirmPasswordVaultModal.find('input[name="user_password"]').val('');
    // }
    // ).
    // fail(function(error)
    // {
    // alert_float('danger', JSON.parse(error.responseText).error_msg);
    // }
    // )
    // ;
    return false;
  }

  /// <inheritdoc/>
  protected override void OnInitialized()
  {
    // LocalStorage.SetItem("name", "John Smith");
    // var name = LocalStorage.GetItem<string>("name");
    // Console.WriteLine("admin area check");
  }

  protected void OnAfterRenderAsync(bool firstRender)
  {
    if (!firstRender) return;
  }
}
