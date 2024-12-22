const StorageUtil = {
  // Save data to localStorage
  setLocal: function (key, value) {
    try {
      const data = JSON.stringify(value);
      localStorage.setItem(key, data);
    } catch (error) {
      console.error("Error saving to localStorage:", error);
    }
  },

  // Get data from localStorage
  getLocal: function (key) {
    try {
      const data = localStorage.getItem(key);
      return data ? JSON.parse(data) : null;
    } catch (error) {
      console.error("Error reading from localStorage:", error);
      return null;
    }
  },

  // Remove data from localStorage
  removeLocal: function (key) {
    try {
      localStorage.removeItem(key);
    } catch (error) {
      console.error("Error removing from localStorage:", error);
    }
  },

  // Save data to sessionStorage
  setSession: function (key, value) {
    try {
      const data = JSON.stringify(value);
      sessionStorage.setItem(key, data);
    } catch (error) {
      console.error("Error saving to sessionStorage:", error);
    }
  },

  // Get data from sessionStorage
  getSession: function (key) {
    try {
      const data = sessionStorage.getItem(key);
      return data ? JSON.parse(data) : null;
    } catch (error) {
      console.error("Error reading from sessionStorage:", error);
      return null;
    }
  },

  // Remove data from sessionStorage
  removeSession: function (key) {
    try {
      sessionStorage.removeItem(key);
    } catch (error) {
      console.error("Error removing from sessionStorage:", error);
    }
  },

  // Clear all data in localStorage
  clearLocal: function () {
    try {
      localStorage.clear();
    } catch (error) {
      console.error("Error clearing localStorage:", error);
    }
  },

  // Clear all data in sessionStorage
  clearSession: function () {
    try {
      sessionStorage.clear();
    } catch (error) {
      console.error("Error clearing sessionStorage:", error);
    }
  }
};
//
// // Usage examples:
// // Set data in localStorage
// StorageUtil.setLocal("user", {name: "John", age: 30});
// // Get data from localStorage
// const user = StorageUtil.getLocal("user");
// console.log("User from localStorage:", user);
// // Remove data from localStorage
// StorageUtil.removeLocal("user");
// // Set data in sessionStorage
// StorageUtil.setSession("sessionData", {token: "abc123"});
// // Get data from sessionStorage
// const sessionData = StorageUtil.getSession("sessionData");
// console.log("Data from sessionStorage:", sessionData);
// // Remove data from sessionStorage
// StorageUtil.removeSession("sessionData");
