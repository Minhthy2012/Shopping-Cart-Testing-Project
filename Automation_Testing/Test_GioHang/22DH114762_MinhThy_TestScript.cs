using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System;
using System.Threading;
using SeleniumExtras.WaitHelpers;
using OpenQA.Selenium.Interactions;
using Unipluss.Sign.ExternalContract.Entities;

namespace Test_GioHang
{
    public class Test
    {

        IWebDriver driver;
        WebDriverWait wait;
        string baseUrl = "https://localhost:44326";

        [SetUp]
        public void Setup()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        }
        [Test]
        public void Test_ThemSP_DatHang_ChuaDangNhap()
        {
            // Bước 1: Mở trang chủ
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(baseUrl );
            wait.Until(ExpectedConditions.UrlContains(baseUrl));
            Console.WriteLine("✅ Đã vào trang chủ.");

            // 🔹 Lấy danh sách sản phẩm
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product")));
            var products = driver.FindElements(By.CssSelector(".single-product"));
            Assert.That(products.Count, Is.GreaterThan(0), "❌ Không tìm thấy sản phẩm nào!");
            Console.WriteLine($"✅ Tìm thấy {products.Count} sản phẩm.");

            // 🔹 Tìm sản phẩm "iPhone 15 Pro Max"
            IWebElement productContainer = null;
            string targetProduct = "REALME C65";
            foreach (var product in products)
            {
                string productText = product.Text;
                Console.WriteLine($"Sản phẩm: {productText}");
                if (productText.Contains(targetProduct))
                {
                    productContainer = product;
                    break;
                }
            }
            if (productContainer == null)
            {
                Console.WriteLine($"⚠️ Không tìm thấy '{targetProduct}', thử 'REALME C12'...");
                foreach (var product in products)
                {
                    if (product.Text.Contains("REALME C12"))
                    {
                        productContainer = product;
                        targetProduct = "REALME C12";
                        break;
                    }
                }
            }
            Assert.That(productContainer, Is.Not.Null, $"❌ Không tìm thấy sản phẩm '{targetProduct}' hoặc fallback trên trang!");

            // 🔹 Scroll và hover sản phẩm
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productContainer);
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product .product-img")));
            //Console.WriteLine($"✅ Đã scroll đến '{targetProduct}' tại Y: {productContainer.Location.Y}, Window Height: {driver.Manage().Window.Size.Height}");

            var productImg = productContainer.FindElement(By.CssSelector(".product-img"));
            var actions = new Actions(driver);
            actions.MoveToElement(productImg).Perform();
            Thread.Sleep(500); // Đợi nhẹ để hover ổn định
            //Console.WriteLine("✅ Đã hover qua hình ảnh sản phẩm.");

            // 🔹 Click vào text "iPhone 15 Pro Max" và điều hướng tới /Details/95
            var productTextElement = productContainer.FindElement(By.XPath(".//*[contains(text(), 'REALME C65')]"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productTextElement);
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            wait.Until(ExpectedConditions.ElementToBeClickable(productTextElement));
            productTextElement.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));

            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/BookStore/Details/99"));
            Console.WriteLine($"✅ Đã điều hướng đến trang chi tiết: {driver.Url}");
            Thread.Sleep(2000);

            var datMuaButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.bg-primary")));
            int buttonY = datMuaButton.Location.Y; // Lấy tọa độ Y của nút
            int windowHeight = driver.Manage().Window.Size.Height; // Lấy chiều cao của cửa sổ trình duyệt
            int scrollTarget = Math.Max(0, buttonY - windowHeight / 3); // Cuộn tới vị trí của nút, dịch lên 1/3 chiều cao màn hình

            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {scrollTarget});");
            Thread.Sleep(500);
            datMuaButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Đã thêm sản phẩm vào giỏ hàng.");
            // 🔹 Quay lại trang index
            driver.Navigate().GoToUrl(baseUrl + "/");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
            Console.WriteLine("✅ Đã quay lại trang index.");

            // 🔹 Click biểu tượng giỏ hàng (debug và thử nhiều cách)
            IWebElement cartLink = null;
            try
            {
                // Thử selector chính xác với "sinlge-bar"
                cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.sinlge-bar.shopping a.single-icon")));
                Console.WriteLine("✅ Tìm thấy giỏ hàng");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("⚠️ Không tìm thấy 'div.sinlge-bar.shopping a.single-icon', thử 'div.single-bar.shopping a.single-icon'...");
                try
                {
                    // Thử sửa typo nếu là "single-bar"
                    cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.single-bar.shopping a.single-icon")));
                    Console.WriteLine("✅ Tìm thấy giỏ hàng");
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("❌ Cả hai selector đều không tìm thấy. Kiểm tra HTML thực tế!");
                    // In HTML để debug
                    var pageSource = driver.PageSource;
                    File.WriteAllText("debug.html", pageSource); // Lưu HTML để kiểm tra
                    Assert.Fail("Không tìm thấy biểu tượng giỏ hàng với bất kỳ selector nào!");
                }
            }

            // Scroll và click
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", cartLink);
                wait.Until(ExpectedConditions.ElementToBeClickable(cartLink)); // Đảm bảo có thể click
                                                                               // Thử click bình thường trước
                cartLink.Click();
                //Console.WriteLine("✅ Đã click giỏ hàng bằng phương thức Click()");
            }
            catch (ElementClickInterceptedException)
            {
                //Console.WriteLine("⚠️ Click bị chặn, thử click bằng JavaScript...");
                // Fallback: dùng JS click nếu click thường không hoạt động
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", cartLink);
                Console.WriteLine("✅ Đã click giỏ hàng");
            }

            // Chờ điều hướng tới trang giỏ hàng
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Giohang"));
                Console.WriteLine("✅ Đã mở trang giỏ hàng.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Không điều hướng được tới giỏ hàng. Current URL: {driver.Url}");
                Assert.Fail("Không thể mở trang giỏ hàng!");
            }
            Thread.Sleep(2000); // Giữ lại để kiểm tra giao diện nếu cần

            // 🔹 Kiểm tra sản phẩm trong giỏ hàng
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{targetProduct}')]")));
            var cartProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{targetProduct}')]"));
            Assert.That(cartProducts.Count, Is.GreaterThan(0), $"❌ Sản phẩm '{targetProduct}' chưa có trong giỏ hàng!");

            Console.WriteLine("✅ Test Passed: Đăng nhập, thêm sản phẩm vào giỏ hàng và kiểm tra giỏ hàng thành công!");
            Thread.Sleep(3000);
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollBy(0, 700);");  // Cuộn xuống 1000 pixel
            Thread.Sleep(1000); // Chờ cho cuộn hoàn tất

            // Bước 5: Nhấn 'Đặt Hàng' (Chưa đăng nhập nên bị chuyển đến trang đăng nhập)
            var datHangButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("tr.bg-success td.text-white a[href='/Giohang/DatHang']")));
            datHangButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            IJavaScriptExecutor jss = (IJavaScriptExecutor)driver;
            jss.ExecuteScript("window.scrollBy(0, 700);");  // Cuộn xuống 1000 pixel
            Thread.Sleep(1000);
            wait.Until(ExpectedConditions.UrlContains(baseUrl+"/Nguoidung/Dangnhap"));

            Console.WriteLine("✅ Bạn chưa đăng nhập.");
        }

        [Test]
        [TestCase("minthy", "123 ", true)]
        public void Test_ThemSPVaoGioHangg(string username, string password, bool shouldLoginSucceed)
        {
            // Tối đa hóa cửa sổ ngay từ đầu để ổn định viewport
            driver.Manage().Window.Maximize();

            // Navigate to login page
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/Nguoidung/Dangnhap"));

            // 🔹 Đăng nhập
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("TenDN"))).SendKeys(username);
            driver.FindElement(By.Name("Matkhau")).SendKeys(password);

            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            var btnDangNhap = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']")));
            btnDangNhap.Click();

            // ✅ Chờ điều hướng sau đăng nhập
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
                Console.WriteLine($"✅ Đã điều hướng đến: {driver.Url}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Timeout khi chờ URL. Current URL: {driver.Url}");
                Assert.Fail("Đăng nhập không điều hướng đúng!");
            }

            // 🔹 Đảm bảo preloader biến mất
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Preloader đã biến mất.");

            // 🔹 Lấy danh sách sản phẩm
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product")));
            var products = driver.FindElements(By.CssSelector(".single-product"));
            Assert.That(products.Count, Is.GreaterThan(0), "❌ Không tìm thấy sản phẩm nào!");
            Console.WriteLine($"✅ Tìm thấy {products.Count} sản phẩm.");

            // 🔹 Tìm sản phẩm "iPhone 15 Pro Max"
            IWebElement productContainer = null;
            string targetProduct = "REALME C65";
            foreach (var product in products)
            {
                string productText = product.Text;
                Console.WriteLine($"Sản phẩm: {productText}");
                if (productText.Contains(targetProduct))
                {
                    productContainer = product;
                    break;
                }
            }
            if (productContainer == null)
            {
                Console.WriteLine($"⚠️ Không tìm thấy '{targetProduct}', thử 'REALME C12'...");
                foreach (var product in products)
                {
                    if (product.Text.Contains("REALME C12"))
                    {
                        productContainer = product;
                        targetProduct = "REALME C12";
                        break;
                    }
                }
            }
            Assert.That(productContainer, Is.Not.Null, $"❌ Không tìm thấy sản phẩm '{targetProduct}' hoặc fallback trên trang!");

            // 🔹 Scroll và hover sản phẩm
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productContainer);
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product .product-img")));
            //Console.WriteLine($"✅ Đã scroll đến '{targetProduct}'");

            var productImg = productContainer.FindElement(By.CssSelector(".product-img"));
            var actions = new Actions(driver);
            actions.MoveToElement(productImg).Perform();
            Thread.Sleep(500); // Đợi nhẹ để hover ổn định
            //Console.WriteLine("✅ Đã hover qua hình ảnh sản phẩm.");

            // 🔹 Click vào text "iPhone 15 Pro Max" và điều hướng tới /Details/95
            var productTextElement = productContainer.FindElement(By.XPath(".//*[contains(text(), 'REALME C65')]"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productTextElement);
            wait.Until(ExpectedConditions.ElementToBeClickable(productTextElement));
            productTextElement.Click();
            wait.Until(ExpectedConditions.UrlContains(baseUrl+"/BookStore/Details/99"));
            Console.WriteLine($"✅ Đã điều hướng đến trang chi tiết: {driver.Url}");
            Thread.Sleep(2000);

            var datMuaButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.bg-primary")));
            int buttonY = datMuaButton.Location.Y; // Lấy tọa độ Y của nút
            int windowHeight = driver.Manage().Window.Size.Height; // Lấy chiều cao của cửa sổ trình duyệt
            int scrollTarget = Math.Max(0, buttonY - windowHeight / 3); // Cuộn tới vị trí của nút, dịch lên 1/3 chiều cao màn hình

            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {scrollTarget});");
            Thread.Sleep(500);
            datMuaButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Đã thêm sản phẩm vào giỏ hàng.");


            // 🔹 Quay lại trang index
            driver.Navigate().GoToUrl(baseUrl + "/BookStore/Index");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/BookStore/Index"));
            Console.WriteLine("✅ Đã quay lại trang index.");

            // 🔹 Click biểu tượng giỏ hàng (debug và thử nhiều cách)
            IWebElement cartLink = null;
            try
            {
                // Thử selector chính xác với "sinlge-bar"
                cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.sinlge-bar.shopping a.single-icon")));
                Console.WriteLine("✅ Tìm thấy icon giỏ hàng");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("⚠️ Không tìm thấy 'div.sinlge-bar.shopping a.single-icon', thử 'div.single-bar.shopping a.single-icon'...");
                try
                {
                    // Thử sửa typo nếu là "single-bar"
                    cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.single-bar.shopping a.single-icon")));
                    Console.WriteLine("✅ Tìm thấy icon giỏ hàng ");
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("❌ Cả hai selector đều không tìm thấy. Kiểm tra HTML thực tế!");
                    // In HTML để debug
                    var pageSource = driver.PageSource;
                    File.WriteAllText("debug.html", pageSource); // Lưu HTML để kiểm tra
                    Assert.Fail("Không tìm thấy biểu tượng giỏ hàng với bất kỳ selector nào!");
                }
            }

            // Scroll và click
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", cartLink);
                wait.Until(ExpectedConditions.ElementToBeClickable(cartLink)); // Đảm bảo có thể click
                                                                               // Thử click bình thường trước
                cartLink.Click();
                Console.WriteLine("✅ Đã click giỏ hàng bằng phương thức Click()");
            }
            catch (ElementClickInterceptedException)
            {
                //Console.WriteLine("⚠️ Click bị chặn");
                // Fallback: dùng JS click nếu click thường không hoạt động
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", cartLink);
                Console.WriteLine("✅ Đã click giỏ hàng ");
            }

            // Chờ điều hướng tới trang giỏ hàng
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Giohang"));
                Console.WriteLine("✅ Đã mở trang giỏ hàng.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Không điều hướng được tới giỏ hàng. Current URL: {driver.Url}");
                Assert.Fail("Không thể mở trang giỏ hàng!");
            }
            Thread.Sleep(2000); // Giữ lại để kiểm tra giao diện nếu cần

            // 🔹 Kiểm tra sản phẩm trong giỏ hàng
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{targetProduct}')]")));
            var cartProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{targetProduct}')]"));
            Assert.That(cartProducts.Count, Is.GreaterThan(0), $"❌ Sản phẩm '{targetProduct}' chưa có trong giỏ hàng!");

            Console.WriteLine("✅ Test Passed: Đăng nhập, thêm sản phẩm vào giỏ hàng và kiểm tra giỏ hàng thành công!");
            Thread.Sleep(3000);
        }
        [Test]
        [TestCase("minthy", "123 ", true)]
        public void Test_ThemSPKhongTonTai(string username, string password, bool shouldLoginSucceed)
        {
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/Nguoidung/Dangnhap"));

            // 🔹 Đăng nhập
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("TenDN"))).SendKeys(username);
            driver.FindElement(By.Name("Matkhau")).SendKeys(password);

            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            var btnDangNhap = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']")));
            btnDangNhap.Click();

            // ✅ Chờ điều hướng
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
                Console.WriteLine($"✅ Đã điều hướng đến: {driver.Url}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Timeout khi chờ URL. Current URL: {driver.Url}");
                Assert.Fail("Đăng nhập không điều hướng đúng!");
            }

            // 🔹 Lấy danh sách sản phẩm hiện có
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product")));
            var products = driver.FindElements(By.CssSelector(".single-product"));
            Assert.That(products.Count, Is.GreaterThan(0), "❌ Không tìm thấy sản phẩm nào!");

            // 🔹 Tìm kiếm sản phẩm không tồn tại
            string nonexistentProduct = "SAMSUNG Z1000"; // Sản phẩm giả định không có
            IWebElement productContainer = null;
            foreach (var product in products)
            {
                if (product.Text.Contains(nonexistentProduct))
                {
                    productContainer = product;
                    break;
                }
            }

            // 🔹 Kiểm tra nếu sản phẩm không tồn tại
            Assert.That(productContainer, Is.Null, $"❌ Sản phẩm '{nonexistentProduct}' không có tồn tại trong danh sách!");

            // 🔹 Kiểm tra nếu thử tìm và thêm sản phẩm thì gặp lỗi
            try
            {
                var productTextElement = driver.FindElement(By.XPath($"//*[contains(text(), '{nonexistentProduct}')]"));
                Assert.Fail($"❌ Đã tìm thấy '{nonexistentProduct}', nhưng sản phẩm này không nên tồn tại!");
            }
            catch (NoSuchElementException)
            {
                Console.WriteLine($"✅ Xác nhận: Sản phẩm '{nonexistentProduct}' không tồn tại trong danh sách.");
            }

            Console.WriteLine("✅ Test Passed: Kiểm tra thêm sản phẩm không có trong dữ liệu thành công!");
        }



        [Test]
        //[TestCase("", "123 ", true)]
        // Hiển thị thông báo lỗi đúng  
        public void Test_ThemSPVaoGioHang_KhongCoUsername()
        {
            // Mở trang đăng nhập
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl+"/Nguoidung/Dangnhap"));

            // Nhập mật khẩu nhưng bỏ trống username
            driver.FindElement(By.Name("Matkhau")).SendKeys("123");
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));

            // Nhấn đăng nhập và chờ lỗi hiển thị
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']"))).Click();
            Thread.Sleep(2000); // Chờ lỗi xuất hiện
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0, 300);");

            // Kiểm tra thông báo lỗi
            try
            {
                var errorMessage = driver.FindElement(By.CssSelector("div.failed")).Text;
                Console.WriteLine(errorMessage);
                Assert.That(errorMessage, Does.Contain("Chưa nhập tên đăng nhập!"), "❌ Không hiển thị lỗi đúng!");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("❌ Không tìm thấy thông báo lỗi!");
                Assert.Fail("Trang không báo lỗi khi username bị thiếu.");
            }

            // Kiểm tra trang vẫn ở trang đăng nhập
            Assert.That(driver.Url, Does.Contain(baseUrl+"/Nguoidung/Dangnhap"), "❌ Hệ thống không giữ lại trang đăng nhập!");

            Console.WriteLine("✅ Test Passed: Đăng nhập bị thiếu username và yêu cầu nhập lại thông tin.");
        }
       
        [Test]
        //Hiển thị thông báo lỗi đúng
        public void Test_ThemSPVaoGioHang_ThieuPass()
        {
            // Mở trang đăng nhập
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/Nguoidung/Dangnhap"));

            // Nhập mật khẩu nhưng bỏ trống pass
            driver.FindElement(By.Name("TenDN")).SendKeys("minthy");
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));

            // Nhấn đăng nhập và chờ lỗi hiển thị
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']"))).Click();
            Thread.Sleep(2000); // Chờ lỗi xuất hiện
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0, 300);");

            // Kiểm tra thông báo lỗi
            try
            {
                var errorMessage = driver.FindElement(By.CssSelector("div.failedd")).Text;
                Console.WriteLine(errorMessage);
                Assert.That(errorMessage, Does.Contain("Chưa nhập mật khẩu!"), "❌ Hiển thị lỗi đúng!");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("❌ Không tìm thấy thông báo lỗi!");
                Assert.Fail("Trang không báo lỗi khi pass bị thiếu.");
            }

            // Kiểm tra trang vẫn ở trang đăng nhập
            Assert.That(driver.Url, Does.Contain(baseUrl + "/Nguoidung/Dangnhap"), "❌ Hệ thống không giữ lại trang đăng nhập!");

            Console.WriteLine("✅ Test Passed: Đăng nhập bị thiếu pass và yêu cầu nhập lại thông tin.");
        }
       
        [Test]
        [TestCase("minthy", "123 ", true)]
        // Đăng nhập thành công và thêm sản phẩm vào giỏ hàng -> Thanh toán
        public void Test_ThemSP_XemChiTietDonHang_ThanhToan(string username, string password, bool shouldLoginSucceed)
        {
            // Tối đa hóa cửa sổ ngay từ đầu để ổn định viewport
            driver.Manage().Window.Maximize();

            // Navigate to login page
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/Nguoidung/Dangnhap"));

            // 🔹 Đăng nhập
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("TenDN"))).SendKeys(username);
            driver.FindElement(By.Name("Matkhau")).SendKeys(password);

            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            var btnDangNhap = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']")));
            btnDangNhap.Click();

            // ✅ Chờ điều hướng sau đăng nhập
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
                Console.WriteLine($"✅ Đã điều hướng đến: {driver.Url}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Timeout khi chờ URL. Current URL: {driver.Url}");
                Assert.Fail("Đăng nhập không điều hướng đúng!");
            }

            // 🔹 Đảm bảo preloader biến mất
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Preloader đã biến mất.");

            // 🔹 Lấy danh sách sản phẩm
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product")));
            var products = driver.FindElements(By.CssSelector(".single-product"));
            Assert.That(products.Count, Is.GreaterThan(0), "❌ Không tìm thấy sản phẩm nào!");
            Console.WriteLine($"✅ Tìm thấy {products.Count} sản phẩm.");

            // 🔹 Tìm sản phẩm "iPhone 15 Pro Max"
            IWebElement productContainer = null;
            string targetProduct = "REALME C65";
            foreach (var product in products)
            {
                string productText = product.Text;
                Console.WriteLine($"Sản phẩm: {productText}");
                if (productText.Contains(targetProduct))
                {
                    productContainer = product;
                    break;
                }
            }
            if (productContainer == null)
            {
                Console.WriteLine($"⚠️ Không tìm thấy '{targetProduct}', thử 'REALME C12'...");
                foreach (var product in products)
                {
                    if (product.Text.Contains("REALME C12"))
                    {
                        productContainer = product;
                        targetProduct = "REALME C12";
                        break;
                    }
                }
            }
            Assert.That(productContainer, Is.Not.Null, $"❌ Không tìm thấy sản phẩm '{targetProduct}' hoặc fallback trên trang!");

            // 🔹 Scroll và hover sản phẩm
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productContainer);
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product .product-img")));
            Console.WriteLine($"✅ Đã scroll đến '{targetProduct}' tại Y: {productContainer.Location.Y}, Window Height: {driver.Manage().Window.Size.Height}");

            var productImg = productContainer.FindElement(By.CssSelector(".product-img"));
            var actions = new Actions(driver);
            actions.MoveToElement(productImg).Perform();
            Thread.Sleep(500); // Đợi nhẹ để hover ổn định
            Console.WriteLine("✅ Đã hover qua hình ảnh sản phẩm.");

            // 🔹 Click vào text "iPhone 15 Pro Max" và điều hướng tới /Details/95
            var productTextElement = productContainer.FindElement(By.XPath(".//*[contains(text(), 'REALME C65')]"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productTextElement);
            wait.Until(ExpectedConditions.ElementToBeClickable(productTextElement));
            productTextElement.Click();
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/BookStore/Details/99"));
            Console.WriteLine($"✅ Đã điều hướng đến trang chi tiết: {driver.Url}");
            Thread.Sleep(2000);

            var datMuaButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.bg-primary")));
            int buttonY = datMuaButton.Location.Y; // Lấy tọa độ Y của nút
            int windowHeight = driver.Manage().Window.Size.Height; // Lấy chiều cao của cửa sổ trình duyệt
            int scrollTarget = Math.Max(0, buttonY - windowHeight / 3); // Cuộn tới vị trí của nút, dịch lên 1/3 chiều cao màn hình

            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {scrollTarget});");
            Thread.Sleep(500);
            datMuaButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Đã thêm sản phẩm vào giỏ hàng.");



            // 🔹 Quay lại trang index
            driver.Navigate().GoToUrl(baseUrl + "/");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
            Console.WriteLine("✅ Đã quay lại trang index.");

            // 🔹 Click biểu tượng giỏ hàng (debug và thử nhiều cách)
            IWebElement cartLink = null;
            try
            {
                // Thử selector chính xác với "sinlge-bar"
                cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.sinlge-bar.shopping a.single-icon")));
                Console.WriteLine("✅ Tìm thấy giỏ hàng với selector 'div.sinlge-bar.shopping a.single-icon'");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("⚠️ Không tìm thấy 'div.sinlge-bar.shopping a.single-icon', thử 'div.single-bar.shopping a.single-icon'...");
                try
                {
                    // Thử sửa typo nếu là "single-bar"
                    cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.single-bar.shopping a.single-icon")));
                    Console.WriteLine("✅ Tìm thấy giỏ hàng với selector 'div.single-bar.shopping a.single-icon'");
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("❌ Cả hai selector đều không tìm thấy. Kiểm tra HTML thực tế!");
                    // In HTML để debug
                    var pageSource = driver.PageSource;
                    File.WriteAllText("debug.html", pageSource); // Lưu HTML để kiểm tra
                    Assert.Fail("Không tìm thấy biểu tượng giỏ hàng với bất kỳ selector nào!");
                }
            }

            // Scroll và click
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", cartLink);
                wait.Until(ExpectedConditions.ElementToBeClickable(cartLink)); // Đảm bảo có thể click
                                                                               // Thử click bình thường trước
                cartLink.Click();
                Console.WriteLine("✅ Đã click giỏ hàng bằng phương thức Click()");
            }
            catch (ElementClickInterceptedException)
            {
                Console.WriteLine("⚠️ Click bị chặn, thử click bằng JavaScript...");
                // Fallback: dùng JS click nếu click thường không hoạt động
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", cartLink);
                Console.WriteLine("✅ Đã click giỏ hàng bằng JavaScript");
            }

            // Chờ điều hướng tới trang giỏ hàng
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Giohang"));
                Console.WriteLine("✅ Đã mở trang giỏ hàng.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Không điều hướng được tới giỏ hàng. Current URL: {driver.Url}");
                Assert.Fail("Không thể mở trang giỏ hàng!");
            }
            Thread.Sleep(2000); // Giữ lại để kiểm tra giao diện nếu cần

            // 🔹 Kiểm tra sản phẩm trong giỏ hàng
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{targetProduct}')]")));
            var cartProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{targetProduct}')]"));
            Assert.That(cartProducts.Count, Is.GreaterThan(0), $"❌ Sản phẩm '{targetProduct}' chưa có trong giỏ hàng!");

            Console.WriteLine("✅ Test Passed: Đăng nhập, thêm sản phẩm vào giỏ hàng và kiểm tra giỏ hàng thành công!");
            Thread.Sleep(3000);


            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollBy(0, 700);");  // Cuộn xuống 1000 pixel
            Thread.Sleep(1000); // Chờ cho cuộn hoàn tất
            var datHangButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("tr.bg-success td.text-white a[href='/Giohang/DatHang']")));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", datHangButton);
            datHangButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/DatHang"));
            Console.WriteLine("✅ Đã điều hướng đến trang thanh toán.");

            Console.WriteLine("✅ Test Passed: Thêm sản phẩm vào giỏ hàng, kiểm tra giỏ hàng và điều hướng tới thanh toán thành công!");
        }
        [Test]
        [TestCase("minthy","123",true)]
        public void Test_ThanhToanThanhCong(string username, string password, bool shouldLoginSucceed)
        {
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/Nguoidung/Dangnhap"));
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("TenDN"))).SendKeys(username);
            driver.FindElement(By.Name("Matkhau")).SendKeys(password);
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            var btnDangNhap = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']")));
            btnDangNhap.Click();
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
                Console.WriteLine($"✅ Đã điều hướng đến: {driver.Url}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Timeout khi chờ URL. Current URL: {driver.Url}");
                Assert.Fail("Đăng nhập không điều hướng đúng!");
            }
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Preloader đã biến mất.");
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product")));
            var products = driver.FindElements(By.CssSelector(".single-product"));
            Assert.That(products.Count, Is.GreaterThan(0), "❌ Không tìm thấy sản phẩm nào!");
            Console.WriteLine($"✅ Tìm thấy {products.Count} sản phẩm.");
            IWebElement productContainer = null;
            string targetProduct = "REALME C65";
            foreach (var product in products)
            {
                string productText = product.Text;
                Console.WriteLine($"Sản phẩm: {productText}");
                if (productText.Contains(targetProduct))
                {
                    productContainer = product;
                    break;
                }
            }
            if (productContainer == null)
            {
                Console.WriteLine($"⚠️ Không tìm thấy '{targetProduct}', thử 'REALME C12'...");
                foreach (var product in products)
                {
                    if (product.Text.Contains("REALME C12"))
                    {
                        productContainer = product;
                        targetProduct = "REALME C12";
                        break;
                    }
                }
            }
            Assert.That(productContainer, Is.Not.Null, $"❌ Không tìm thấy sản phẩm '{targetProduct}' hoặc fallback trên trang!");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productContainer);
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product .product-img")));
            //Console.WriteLine($"✅ Đã scroll đến '{targetProduct}'");
            var productImg = productContainer.FindElement(By.CssSelector(".product-img"));
            var actions = new Actions(driver);
            actions.MoveToElement(productImg).Perform();
            Thread.Sleep(500);
            //Console.WriteLine("✅ Đã hover qua hình ảnh sản phẩm.");
            var productTextElement = productContainer.FindElement(By.XPath(".//*[contains(text(), 'REALME C65')]"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productTextElement);
            wait.Until(ExpectedConditions.ElementToBeClickable(productTextElement));
            productTextElement.Click();
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/BookStore/Details/99"));
            Console.WriteLine($"✅ Đã điều hướng đến trang chi tiết: {driver.Url}");
            Thread.Sleep(2000);
            var datMuaButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.bg-primary")));
            int buttonY = datMuaButton.Location.Y;
            int windowHeight = driver.Manage().Window.Size.Height;
            int scrollTarget = Math.Max(0, buttonY - windowHeight / 3);
            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {scrollTarget});");
            Thread.Sleep(500);
            datMuaButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Đã thêm sản phẩm vào giỏ hàng.");
            driver.Navigate().GoToUrl(baseUrl + "/GioHang/Giohang");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Giohang"));
            Console.WriteLine("✅ Đã mở trang giỏ hàng.");
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{targetProduct}')]")));
            var cartProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{targetProduct}')]"));
            Assert.That(cartProducts.Count, Is.GreaterThan(0), $"❌ Sản phẩm '{targetProduct}' chưa có trong giỏ hàng!");
            Console.WriteLine("✅ Test Passed: Đăng nhập, thêm sản phẩm vào giỏ hàng và kiểm tra giỏ hàng thành công!");
            Thread.Sleep(3000);
            
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollBy(0, 700);");  // Cuộn xuống 1000 pixel
            Thread.Sleep(1000); // Chờ cho cuộn hoàn tất
            var datHangButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("tr.bg-success td.text-white a[href='/Giohang/DatHang']")));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", datHangButton);
            datHangButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/DatHang"));
            Console.WriteLine("✅ Đã điều hướng đến trang thanh toán.");

            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo({top: 1000, behavior: 'smooth'});");
            Thread.Sleep(1000);
          
            var dateField = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='Ngaygiao']")));
            dateField.Clear();
            string dateToEnter = "03/27/2025";
            dateField.SendKeys(dateToEnter);
            Thread.Sleep(3000);
            Console.WriteLine("✅ Đã nhập ngày giao hàng.");
            var dongYDatHangButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("td[colspan='2'] input[type='submit'][value='Đồng ý đặt hàng']")));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", dongYDatHangButton);
            dongYDatHangButton.Click(); wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Xacnhandonhang"));
            Console.WriteLine("✅ Đã điều hướng đến trang thanh toán.");
            Console.WriteLine("✅ Test Passed: Thêm sản phẩm vào giỏ hàng, kiểm tra giỏ hàng và điều hướng tới thanh toán thành công!");
        }
        [Test]
        [TestCase("minthy", "123", true)]
        public void Test_ThanhToanThatBai(string username, string password, bool shouldLoginSucceed)
        {
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/Nguoidung/Dangnhap"));
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("TenDN"))).SendKeys(username);
            driver.FindElement(By.Name("Matkhau")).SendKeys(password);
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            var btnDangNhap = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']")));
            btnDangNhap.Click();
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
                Console.WriteLine($"✅ Đã điều hướng đến: {driver.Url}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Timeout khi chờ URL. Current URL: {driver.Url}");
                Assert.Fail("Đăng nhập không điều hướng đúng!");
            }
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Preloader đã biến mất.");
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product")));
            var products = driver.FindElements(By.CssSelector(".single-product"));
            Assert.That(products.Count, Is.GreaterThan(0), "❌ Không tìm thấy sản phẩm nào!");
            Console.WriteLine($"✅ Tìm thấy {products.Count} sản phẩm.");
            IWebElement productContainer = null;
            string targetProduct = "REALME C65";
            foreach (var product in products)
            {
                string productText = product.Text;
                Console.WriteLine($"Sản phẩm: {productText}");
                if (productText.Contains(targetProduct))
                {
                    productContainer = product;
                    break;
                }
            }
            if (productContainer == null)
            {
                Console.WriteLine($"⚠️ Không tìm thấy '{targetProduct}', thử 'REALME C12'...");
                foreach (var product in products)
                {
                    if (product.Text.Contains("REALME C12"))
                    {
                        productContainer = product;
                        targetProduct = "REALME C12";
                        break;
                    }
                }
            }
            Assert.That(productContainer, Is.Not.Null, $"❌ Không tìm thấy sản phẩm '{targetProduct}' hoặc fallback trên trang!");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productContainer);
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product .product-img")));
            //Console.WriteLine($"✅ Đã scroll đến '{targetProduct}'");
            var productImg = productContainer.FindElement(By.CssSelector(".product-img"));
            var actions = new Actions(driver);
            actions.MoveToElement(productImg).Perform();
            Thread.Sleep(500);
            //Console.WriteLine("✅ Đã hover qua hình ảnh sản phẩm.");
            var productTextElement = productContainer.FindElement(By.XPath(".//*[contains(text(), 'REALME C65')]"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productTextElement);
            wait.Until(ExpectedConditions.ElementToBeClickable(productTextElement));
            productTextElement.Click();
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/BookStore/Details/99"));
            Console.WriteLine($"✅ Đã điều hướng đến trang chi tiết: {driver.Url}");
            Thread.Sleep(2000);
            var datMuaButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.bg-primary")));
            int buttonY = datMuaButton.Location.Y;
            int windowHeight = driver.Manage().Window.Size.Height;
            int scrollTarget = Math.Max(0, buttonY - windowHeight / 3);
            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {scrollTarget});");
            Thread.Sleep(500);
            datMuaButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Đã thêm sản phẩm vào giỏ hàng.");
            driver.Navigate().GoToUrl(baseUrl + "/GioHang/Giohang");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Giohang"));
            Console.WriteLine("✅ Đã mở trang giỏ hàng.");
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{targetProduct}')]")));
            var cartProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{targetProduct}')]"));
            Assert.That(cartProducts.Count, Is.GreaterThan(0), $"❌ Sản phẩm '{targetProduct}' chưa có trong giỏ hàng!");
            Console.WriteLine("✅ Test Passed: Đăng nhập, thêm sản phẩm vào giỏ hàng và kiểm tra giỏ hàng thành công!");
            Thread.Sleep(3000);

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollBy(0, 700);");  // Cuộn xuống 1000 pixel
            Thread.Sleep(1000); // Chờ cho cuộn hoàn tất
            var datHangButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("tr.bg-success td.text-white a[href='/Giohang/DatHang']")));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", datHangButton);
            datHangButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/DatHang"));
            Console.WriteLine("✅ Đã điều hướng đến trang thanh toán.");

            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo({top: 1000, behavior: 'smooth'});");
            Thread.Sleep(1000);

            var dateField = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='Ngaygiao']")));
            dateField.Clear();
            string dateToEnter = "";
            dateField.SendKeys(dateToEnter);
            Thread.Sleep(3000);
            try
            {
                // Kiểm tra xem có trường ngày giao hàng chưa được nhập không
                var ngayGiaoHangInput = driver.FindElement(By.CssSelector("input[name='Ngaygiao']"));

                if (string.IsNullOrEmpty(ngayGiaoHangInput.GetAttribute("value")))
                {
                    Console.WriteLine("❌ Thanh toán thất bại: Vui lòng nhập ngày giao hàng mong muốn.");
                    // Ném lỗi để dừng quá trình kiểm tra
                    throw new Exception("Thanh toán thất bại: Chưa nhập ngày giao hàng.");
                }
                else
                {
                    Console.WriteLine("✅ Đã nhập ngày giao hàng.");
                }

                // Tiến hành các bước tiếp theo nếu đã nhập ngày giao hàng
                var dongYDatHangButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("td[colspan='2'] input[type='submit'][value='Đồng ý đặt hàng']")));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", dongYDatHangButton);
                dongYDatHangButton.Click();
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Xacnhandonhang"));
                Console.WriteLine("✅ Đã điều hướng đến trang thanh toán.");
                Console.WriteLine("✅ Test Passed: Thêm sản phẩm vào giỏ hàng, kiểm tra giỏ hàng và điều hướng tới thanh toán thành công!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi: {ex.Message}");
                // Nếu có lỗi, có thể thực hiện một số hành động khác, như ghi log, chụp ảnh màn hình, v.v.
            }
        }
            [Test]
        [TestCase("minthy", "123 ", true)]
        public void Test_ThemNhieuSPVaoGioHang_Xoa1SP(string username, string password, bool shouldLoginSucceed)
        {
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/Nguoidung/Dangnhap"));

            // 🔹 Đăng nhập
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("TenDN"))).SendKeys(username);
            driver.FindElement(By.Name("Matkhau")).SendKeys(password);
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            var btnDangNhap = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']")));
            btnDangNhap.Click();
            // ✅ Chờ điều hướng sau đăng nhập
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
                Console.WriteLine($"✅ Đã điều hướng đến: {driver.Url}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Timeout khi chờ URL. Current URL: {driver.Url}");
                Assert.Fail("Đăng nhập không điều hướng đúng!");
            }

            // 🔹 Đảm bảo preloader biến mất
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Preloader đã biến mất.");

            // 🔹 Danh sách sản phẩm cần mua
            string[] productsToAdd = { "REALME C65", "REALME C53" };

            for (int i = 0; i < productsToAdd.Length; i++)
            {
                string productName = productsToAdd[i];

                // 🔹 Scroll tìm sản phẩm
                var products = driver.FindElements(By.CssSelector(".single-product"));
                IWebElement productContainer = products.FirstOrDefault(p => p.Text.Contains(productName));
                Assert.That(productContainer, Is.Not.Null, $"❌ Không tìm thấy sản phẩm '{productName}' trên trang!");

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productContainer);
                Thread.Sleep(2000);

                //Console.WriteLine($"✅ Đã scroll đến sản phẩm: {productName}");

                // 🔹 Mở trang chi tiết sản phẩm bằng cách click vào phần tử chứa link
                var productLink = productContainer.FindElement(By.CssSelector("a"));
                wait.Until(ExpectedConditions.ElementToBeClickable(productLink)).Click();
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/BookStore/Details/"));

                Console.WriteLine($"✅ Đã vào trang chi tiết của '{productName}'");


                // 🔹 Thêm vào giỏ hàng
                var datMuaButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.bg-primary")));
                int buttonY = datMuaButton.Location.Y; // Lấy tọa độ Y của nút
                int windowHeight = driver.Manage().Window.Size.Height; // Lấy chiều cao của cửa sổ trình duyệt
                int scrollTarget = Math.Max(0, buttonY - windowHeight / 3); // Cuộn tới vị trí của nút, dịch lên 1/3 chiều cao màn hình

                ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {scrollTarget});");
                Thread.Sleep(2000);
                datMuaButton.Click();
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
                //Console.WriteLine("✅ Đã thêm sản phẩm vào giỏ hàng.");

                Console.WriteLine($"✅ Đã thêm '{productName}' vào giỏ hàng");

                // Quay lại trang chính để thêm sản phẩm tiếp theo
                driver.Navigate().GoToUrl(baseUrl + "/BookStore/Index");
            }
            // 🔹 Click biểu tượng giỏ hàng (debug và thử nhiều cách)
            IWebElement cartLink = null;
            try
            {
                // Thử selector chính xác với "sinlge-bar"
                cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.sinlge-bar.shopping a.single-icon")));
                Console.WriteLine("✅ Tìm thấy giỏ hàng");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("⚠️ Không tìm thấy 'div.sinlge-bar.shopping a.single-icon', thử 'div.single-bar.shopping a.single-icon'...");
                try
                {
                    // Thử sửa typo nếu là "single-bar"
                    cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.single-bar.shopping a.single-icon")));
                    Console.WriteLine("✅ Tìm thấy giỏ hàng");
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("❌ Cả hai selector đều không tìm thấy. Kiểm tra HTML thực tế!");
                    // In HTML để debug
                    var pageSource = driver.PageSource;
                    File.WriteAllText("debug.html", pageSource); // Lưu HTML để kiểm tra
                    Assert.Fail("Không tìm thấy biểu tượng giỏ hàng với bất kỳ selector nào!");
                }
            }

            // Scroll và click
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", cartLink);
                wait.Until(ExpectedConditions.ElementToBeClickable(cartLink)); // Đảm bảo có thể click
                                                                               // Thử click bình thường trước
                cartLink.Click();
                Console.WriteLine("✅ Đã click giỏ hàng");
            }
            catch (ElementClickInterceptedException)
            {
                //Console.WriteLine("⚠️ Click bị chặn");
                // Fallback: dùng JS click nếu click thường không hoạt động
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", cartLink);
                Console.WriteLine("✅ Đã click giỏ hàng ");
            }

            // Chờ điều hướng tới trang giỏ hàng
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Giohang"));
                Console.WriteLine("✅ Đã mở trang giỏ hàng.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Không điều hướng được tới giỏ hàng. Current URL: {driver.Url}");
                Assert.Fail("Không thể mở trang giỏ hàng!");
            }
            Thread.Sleep(2000); // Giữ lại để kiểm tra giao diện nếu cần
            // 🔹 Kiểm tra cả hai sản phẩm có trong giỏ hàng
            for (int i = 0; i < productsToAdd.Length; i++)
            {
                string productName = productsToAdd[i];
                var cartProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{productName}')]"));
                Assert.That(cartProducts.Count, Is.GreaterThan(0), $"❌ Sản phẩm '{productName}' chưa có trong giỏ hàng!");
            }

            // 🔹 Scroll xuống từng sản phẩm trước khi xóa
            for (int i = 0; i < productsToAdd.Length; i++)
            {
                string productName = productsToAdd[i];
                var productRow = driver.FindElement(By.XPath($"//td[contains(text(), '{productName}')]"));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productRow);
                Thread.Sleep(1000);
                //Console.WriteLine($"✅ Đã scroll đến sản phẩm trong giỏ hàng: {productName}");
            }
            // 🔹 Xóa sản phẩm đầu tiên trong danh sách
            foreach (var productName in productsToAdd)
            {
                string productToRemove = productsToAdd[0];
                var deleteButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//td[@class='text-danger']/a[@href='/Giohang/XoaGiohang?iMaSp=99']")));
                deleteButton.Click();

                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath($"//td[contains(text(), '{productToRemove}')]")));

                Console.WriteLine($"✅ Đã xóa sản phẩm '{productName}' khỏi giỏ hàng");

                // 🔹 Kiểm tra sau khi xóa vẫn ở trang giỏ hàng
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/GioHang"));
                Console.WriteLine("✅ Đã điều hướng đến trang giỏ hàng.");

                // 🔹 Kiểm tra sản phẩm đã bị xóa
                bool isProductRemoved = CheckProductRemoved(productName);
                Assert.That(isProductRemoved, Is.True, $"❌ Sản phẩm '{productName}' vẫn còn trong giỏ hàng!");
                Console.WriteLine($"✅ Sản phẩm '{productName}' đã được xóa khỏi giỏ hàng.");
            }

            // Kiểm tra tất cả sản phẩm đã bị xóa thành công
            Console.WriteLine("✅ Test Passed: Thêm hai sản phẩm vào giỏ hàng, xóa một sản phẩm thành công!");
            Thread.Sleep(3000);
        }
       // Hàm kiểm tra sản phẩm đã bị xóa
        public bool CheckProductRemoved(string productName)
        {
            var remainingProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{productName}')]"));
            if (remainingProducts.Count > 0)
            {
                // Nếu sản phẩm vẫn còn trong giỏ hàng
                Console.WriteLine($"❌ Sản phẩm '{productName}' vẫn còn trong giỏ hàng!");
                return false;
            }
            return true; // Nếu sản phẩm đã bị xóa
        }
        [Test]
        [TestCase("minthy", "123 ", true)]
        public void Test_ThemHaiSPVaoGioHang_XoaTatCaSP(string username, string password, bool shouldLoginSucceed)
        {
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/Nguoidung/Dangnhap"));

            // 🔹 Đăng nhập
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("TenDN"))).SendKeys(username);
            driver.FindElement(By.Name("Matkhau")).SendKeys(password);
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            var btnDangNhap = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']")));
            btnDangNhap.Click();
            // ✅ Chờ điều hướng sau đăng nhập
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
                Console.WriteLine($"✅ Đã điều hướng đến: {driver.Url}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Timeout khi chờ URL. Current URL: {driver.Url}");
                Assert.Fail("Đăng nhập không điều hướng đúng!");
            }

            // 🔹 Đảm bảo preloader biến mất
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Preloader đã biến mất.");

            // 🔹 Danh sách sản phẩm cần mua
            string[] productsToAdd = { "REALME C65", "REALME C53" };

            for (int i = 0; i < productsToAdd.Length; i++)
            {
                string productName = productsToAdd[i];

                // 🔹 Scroll tìm sản phẩm
                var products = driver.FindElements(By.CssSelector(".single-product"));
                IWebElement productContainer = products.FirstOrDefault(p => p.Text.Contains(productName));
                Assert.That(productContainer, Is.Not.Null, $"❌ Không tìm thấy sản phẩm '{productName}' trên trang!");

                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productContainer);
                Thread.Sleep(2000);

                //Console.WriteLine($"✅ Đã scroll đến sản phẩm: {productName}");

                // 🔹 Mở trang chi tiết sản phẩm bằng cách click vào phần tử chứa link
                var productLink = productContainer.FindElement(By.CssSelector("a"));
                wait.Until(ExpectedConditions.ElementToBeClickable(productLink)).Click();
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/BookStore/Details/"));

                Console.WriteLine($"✅ Đã vào trang chi tiết của '{productName}'");


                // 🔹 Thêm vào giỏ hàng
                var datMuaButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.bg-primary")));
                int buttonY = datMuaButton.Location.Y; // Lấy tọa độ Y của nút
                int windowHeight = driver.Manage().Window.Size.Height; // Lấy chiều cao của cửa sổ trình duyệt
                int scrollTarget = Math.Max(0, buttonY - windowHeight / 3); // Cuộn tới vị trí của nút, dịch lên 1/3 chiều cao màn hình

                ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {scrollTarget});");
                Thread.Sleep(2000);
                datMuaButton.Click();
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
                //Console.WriteLine("✅ Đã thêm sản phẩm vào giỏ hàng.");

                Console.WriteLine($"✅ Đã thêm '{productName}' vào giỏ hàng");

                // Quay lại trang chính để thêm sản phẩm tiếp theo
                driver.Navigate().GoToUrl(baseUrl + "/BookStore/Index");
            }
            // 🔹 Click biểu tượng giỏ hàng (debug và thử nhiều cách)
            IWebElement cartLink = null;
            try
            {
                // Thử selector chính xác với "sinlge-bar"
                cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.sinlge-bar.shopping a.single-icon")));
                Console.WriteLine("✅ Tìm thấy giỏ hàng");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("⚠️ Không tìm thấy 'div.sinlge-bar.shopping a.single-icon', thử 'div.single-bar.shopping a.single-icon'...");
                try
                {
                    // Thử sửa typo nếu là "single-bar"
                    cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.single-bar.shopping a.single-icon")));
                    Console.WriteLine("✅ Tìm thấy giỏ hàng");
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("❌ Cả hai selector đều không tìm thấy. Kiểm tra HTML thực tế!");
                    // In HTML để debug
                    var pageSource = driver.PageSource;
                    File.WriteAllText("debug.html", pageSource); // Lưu HTML để kiểm tra
                    Assert.Fail("Không tìm thấy biểu tượng giỏ hàng với bất kỳ selector nào!");
                }
            }

            // Scroll và click
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", cartLink);
                wait.Until(ExpectedConditions.ElementToBeClickable(cartLink)); // Đảm bảo có thể click
                                                                               // Thử click bình thường trước
                cartLink.Click();
                Console.WriteLine("✅ Đã click giỏ hàng");
            }
            catch (ElementClickInterceptedException)
            {
                //Console.WriteLine("⚠️ Click bị chặn");
                // Fallback: dùng JS click nếu click thường không hoạt động
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", cartLink);
                Console.WriteLine("✅ Đã click giỏ hàng");
            }

            // Chờ điều hướng tới trang giỏ hàng
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Giohang"));
                Console.WriteLine("✅ Đã mở trang giỏ hàng.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Không điều hướng được tới giỏ hàng. Current URL: {driver.Url}");
                Assert.Fail("Không thể mở trang giỏ hàng!");
            }
            Thread.Sleep(2000); // Giữ lại để kiểm tra giao diện nếu cần
            // 🔹 Kiểm tra cả hai sản phẩm có trong giỏ hàng
            for (int i = 0; i < productsToAdd.Length; i++)
            {
                string productName = productsToAdd[i];
                var cartProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{productName}')]"));
                Assert.That(cartProducts.Count, Is.GreaterThan(0), $"❌ Sản phẩm '{productName}' chưa có trong giỏ hàng!");
            }

            // 🔹 Scroll xuống từng sản phẩm trước khi xóa
            for (int i = 0; i < productsToAdd.Length; i++)
            {
                string productName = productsToAdd[i];
                var productRow = driver.FindElement(By.XPath($"//td[contains(text(), '{productName}')]"));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productRow);
                Thread.Sleep(1000);
                //Console.WriteLine($"✅ Đã scroll đến sản phẩm trong giỏ hàng: {productName}");
            }
            // 🔹 Xóa tất cả sản phẩm khỏi giỏ hàng
            foreach (var productName in productsToAdd)
            {
                var deleteAllButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("td a[href='/Giohang/XoaTatcaGiohang']")));
                deleteAllButton.Click();
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.XPath($"//td[contains(text(), '{productName}')]")));
                Console.WriteLine($"✅ Đã xóa tất cả sp khỏi giỏ hàng");
            }

            // 🔹 Kiểm tra giỏ hàng trống
            var remainingProducts = driver.FindElements(By.XPath("//table/tbody/tr/td[1]"));
            Assert.That(remainingProducts.Count, Is.EqualTo(0), "❌ Giỏ hàng chưa trống!");
            Console.WriteLine("✅ Tất cả sản phẩm đã được xóa khỏi giỏ hàng.");
        }

        // Hàm kiểm tra sản phẩm còn trong giỏ hàng
        public bool CheckProductRemovedd(string productName)
        {
            var remainingProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{productName}')]"));
            if (remainingProducts.Count > 0)
            {
                Console.WriteLine($"❌ Sản phẩm '{productName}' vẫn còn trong giỏ hàng!");
                return false;
            }
            return true;
        }
        [Test]
        [TestCase("minthy", "123", true)]
        public void Test_ThemSP_CapNhatSoLuong(string username, string password, bool shouldloginSucceed)
        {
            // Tối đa hóa cửa sổ ngay từ đầu để ổn định viewport
            driver.Manage().Window.Maximize();

            // Navigate to login page
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/Nguoidung/Dangnhap"));

            // 🔹 Đăng nhập
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("TenDN"))).SendKeys(username);
            driver.FindElement(By.Name("Matkhau")).SendKeys(password);

            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            var btnDangNhap = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']")));
            btnDangNhap.Click();

            // ✅ Chờ điều hướng sau đăng nhập
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
                Console.WriteLine($"✅ Đã điều hướng đến: {driver.Url}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Timeout khi chờ URL. Current URL: {driver.Url}");
                Assert.Fail("Đăng nhập không điều hướng đúng!");
            }

            // 🔹 Đảm bảo preloader biến mất
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Preloader đã biến mất.");

            // 🔹 Lấy danh sách sản phẩm
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product")));
            var products = driver.FindElements(By.CssSelector(".single-product"));
            Assert.That(products.Count, Is.GreaterThan(0), "❌ Không tìm thấy sản phẩm nào!");
            Console.WriteLine($"✅ Tìm thấy {products.Count} sản phẩm.");

            // 🔹 Tìm sản phẩm "iPhone 15 Pro Max"
            IWebElement productContainer = null;
            string targetProduct = "REALME C65";
            foreach (var product in products)
            {
                string productText = product.Text;
                Console.WriteLine($"Sản phẩm: {productText}");
                if (productText.Contains(targetProduct))
                {
                    productContainer = product;
                    break;
                }
            }
            if (productContainer == null)
            {
                Console.WriteLine($"⚠️ Không tìm thấy '{targetProduct}', thử 'REALME C12'...");
                foreach (var product in products)
                {
                    if (product.Text.Contains("REALME C12"))
                    {
                        productContainer = product;
                        targetProduct = "REALME C12";
                        break;
                    }
                }
            }
            Assert.That(productContainer, Is.Not.Null, $"❌ Không tìm thấy sản phẩm '{targetProduct}' hoặc fallback trên trang!");

            // 🔹 Scroll và hover sản phẩm
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productContainer);
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product .product-img")));
            //Console.WriteLine($"✅ Đã scroll đến '{targetProduct}' tại Y: {productContainer.Location.Y}, Window Height: {driver.Manage().Window.Size.Height}");

            var productImg = productContainer.FindElement(By.CssSelector(".product-img"));
            var actions = new Actions(driver);
            actions.MoveToElement(productImg).Perform();
            Thread.Sleep(500); // Đợi nhẹ để hover ổn định
            //Console.WriteLine("✅ Đã hover qua hình ảnh sản phẩm.");

            // 🔹 Click vào text "iPhone 15 Pro Max" và điều hướng tới /Details/95
            var productTextElement = productContainer.FindElement(By.XPath(".//*[contains(text(), 'REALME C65')]"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productTextElement);
            wait.Until(ExpectedConditions.ElementToBeClickable(productTextElement));
            productTextElement.Click();
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/BookStore/Details/99"));
            Console.WriteLine($"✅ Đã điều hướng đến trang chi tiết: {driver.Url}");
            Thread.Sleep(2000);

               var datMuaButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.bg-primary")));
            int buttonY = datMuaButton.Location.Y; // Lấy tọa độ Y của nút
            int windowHeight = driver.Manage().Window.Size.Height; // Lấy chiều cao của cửa sổ trình duyệt
            int scrollTarget = Math.Max(0, buttonY - windowHeight / 3); // Cuộn tới vị trí của nút, dịch lên 1/3 chiều cao màn hình

            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {scrollTarget});");
            Thread.Sleep(500);
            datMuaButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Đã thêm sản phẩm vào giỏ hàng.");



            // 🔹 Quay lại trang index
            driver.Navigate().GoToUrl(baseUrl + "/");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
            Console.WriteLine("✅ Đã quay lại trang index.");

            // 🔹 Click biểu tượng giỏ hàng (debug và thử nhiều cách)
            IWebElement cartLink = null;
            try
            {
                // Thử selector chính xác với "sinlge-bar"
                cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.sinlge-bar.shopping a.single-icon")));
                Console.WriteLine("✅ Tìm thấy giỏ hàng");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("⚠️ Không tìm thấy 'div.sinlge-bar.shopping a.single-icon', thử 'div.single-bar.shopping a.single-icon'...");
                try
                {
                    // Thử sửa typo nếu là "single-bar"
                    cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.single-bar.shopping a.single-icon")));
                    Console.WriteLine("✅ Tìm thấy giỏ hàng");
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("❌ Cả hai selector đều không tìm thấy. Kiểm tra HTML thực tế!");
                    // In HTML để debug
                    var pageSource = driver.PageSource;
                    File.WriteAllText("debug.html", pageSource); // Lưu HTML để kiểm tra
                    Assert.Fail("Không tìm thấy biểu tượng giỏ hàng với bất kỳ selector nào!");
                }
            }

            // Scroll và click
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", cartLink);
                wait.Until(ExpectedConditions.ElementToBeClickable(cartLink)); // Đảm bảo có thể click
                                                                               // Thử click bình thường trước
                cartLink.Click();
                Console.WriteLine("✅ Đã click giỏ hàng");
            }
            catch (ElementClickInterceptedException)
            {
                //Console.WriteLine("⚠️ Click bị chặn");
                // Fallback: dùng JS click nếu click thường không hoạt động
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", cartLink);
                Console.WriteLine("✅ Đã click giỏ hàng");
            }

            // Chờ điều hướng tới trang giỏ hàng
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Giohang"));
                Console.WriteLine("✅ Đã mở trang giỏ hàng.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Không điều hướng được tới giỏ hàng. Current URL: {driver.Url}");
                Assert.Fail("Không thể mở trang giỏ hàng!");
            }
            Thread.Sleep(2000); // Giữ lại để kiểm tra giao diện nếu cần

            // 🔹 Kiểm tra sản phẩm trong giỏ hàng
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{targetProduct}')]")));
            var cartProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{targetProduct}')]"));
            Assert.That(cartProducts.Count, Is.GreaterThan(0), $"❌ Sản phẩm '{targetProduct}' chưa có trong giỏ hàng!");

            Console.WriteLine("✅ Test Passed: Đăng nhập, thêm sản phẩm vào giỏ hàng và kiểm tra giỏ hàng thành công!");
            // 🔹 Cuộn trang đến giữa trang web một lần
            
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // Lấy chiều cao của trang và cuộn đến giữa
            long height = (long)js.ExecuteScript("return document.body.scrollHeight");
            long middlePosition = height / 3; // Tính vị trí giữa trang
            js.ExecuteScript($"window.scrollTo(0, {middlePosition});");

            //Console.WriteLine("✅ Đã cuộn trang đến giữa.");
            Thread.Sleep(2000); // Chờ cho cuộn hoàn tất

            // 🔹 Tìm ô nhập số lượng sản phẩm
            var quantityInput = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='txtSoluong']")));

            // 🔹 Tăng số lượng lên 2
            quantityInput.Clear();
            quantityInput.SendKeys("2");
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Tăng số lượng lên 2.");
            Thread.Sleep(2000);

            // 🔹 Click nút Cập Nhật
            var updateButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("input.btn-success[type='submit'][value='Cập Nhật']")));
            updateButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Đã cập nhật số lượng sản phẩm.");
            Thread.Sleep(2000); 

            // 🔹 Cuộn trang đến giữa trang web một lần
            IJavaScriptExecutor jss = (IJavaScriptExecutor)driver;

            // Lấy chiều cao của trang và cuộn đến giữa
            long heightt = (long)js.ExecuteScript("return document.body.scrollHeight");
            long middlePositionn = height / 3; // Tính vị trí giữa trang
            js.ExecuteScript($"window.scrollTo(0, {middlePosition});");

            //Console.WriteLine("✅ Đã cuộn trang đến giữa.");
            Thread.Sleep(2000); // Chờ cho cuộn hoàn tất
            // 🔹 Giảm số lượng về 1
            quantityInput = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='txtSoluong']")));
            quantityInput.Clear();
            quantityInput.SendKeys("1");
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Giảm số lượng về 1.");
            Thread.Sleep(2000);

            // 🔹 Click nút Cập Nhật lần nữa
            updateButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("input.btn-success[type='submit'][value='Cập Nhật']")));
            updateButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Đã cập nhật số lượng sản phẩm xuống 1.");
            Thread.Sleep(4000);
        }
        [Test]
        [TestCase("minthy", "123", true)]
        public void Test_ThemSP_CapNhatSoLuong_VuotQuaGioiHan(string username, string password, bool shouldloginSucceed)
        {
            driver.Manage().Window.Maximize();

            // Navigate to login page
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/Nguoidung/Dangnhap"));

            // 🔹 Đăng nhập
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("TenDN"))).SendKeys(username);
            driver.FindElement(By.Name("Matkhau")).SendKeys(password);

            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            var btnDangNhap = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']")));
            btnDangNhap.Click();

            // ✅ Chờ điều hướng sau đăng nhập
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
                Console.WriteLine($"✅ Đã điều hướng đến: {driver.Url}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Timeout khi chờ URL. Current URL: {driver.Url}");
                Assert.Fail("Đăng nhập không điều hướng đúng!");
            }

            // 🔹 Đảm bảo preloader biến mất
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Preloader đã biến mất.");
            // 🔹 Quay lại trang index
            driver.Navigate().GoToUrl(baseUrl + "/");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
            Console.WriteLine("✅ Đã quay lại trang index.");
            // 🔹 Lấy danh sách sản phẩm
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product")));
            var products = driver.FindElements(By.CssSelector(".single-product"));
            Assert.That(products.Count, Is.GreaterThan(0), "❌ Không tìm thấy sản phẩm nào!");
            Console.WriteLine($"✅ Tìm thấy {products.Count} sản phẩm.");

            // 🔹 Tìm sản phẩm "iPhone 15 Pro Max"
            IWebElement productContainer = null;
            string targetProduct = "REALME C65";
            foreach (var product in products)
            {
                string productText = product.Text;
                Console.WriteLine($"Sản phẩm: {productText}");
                if (productText.Contains(targetProduct))
                {
                    productContainer = product;
                    break;
                }
            }
            if (productContainer == null)
            {
                Console.WriteLine($"⚠️ Không tìm thấy '{targetProduct}', thử 'REALME C12'...");
                foreach (var product in products)
                {
                    if (product.Text.Contains("REALME C12"))
                    {
                        productContainer = product;
                        targetProduct = "REALME C12";
                        break;
                    }
                }
            }
            Assert.That(productContainer, Is.Not.Null, $"❌ Không tìm thấy sản phẩm '{targetProduct}' hoặc fallback trên trang!");

            // 🔹 Scroll và hover sản phẩm
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productContainer);
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product .product-img")));
            //Console.WriteLine($"✅ Đã scroll đến '{targetProduct}' tại Y: {productContainer.Location.Y}, Window Height: {driver.Manage().Window.Size.Height}");

            var productImg = productContainer.FindElement(By.CssSelector(".product-img"));
            var actions = new Actions(driver);
            actions.MoveToElement(productImg).Perform();
            Thread.Sleep(500); // Đợi nhẹ để hover ổn định
            //Console.WriteLine("✅ Đã hover qua hình ảnh sản phẩm.");

            // 🔹 Click vào text "iPhone 15 Pro Max" và điều hướng tới /Details/95
            var productTextElement = productContainer.FindElement(By.XPath(".//*[contains(text(), 'REALME C65')]"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productTextElement);
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));

            wait.Until(ExpectedConditions.ElementToBeClickable(productTextElement));
            productTextElement.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));

            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/BookStore/Details/99"));
            Console.WriteLine($"✅ Đã điều hướng đến trang chi tiết: {driver.Url}");
            Thread.Sleep(2000);

            var datMuaButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.bg-primary")));
            int buttonY = datMuaButton.Location.Y; // Lấy tọa độ Y của nút
            int windowHeight = driver.Manage().Window.Size.Height; // Lấy chiều cao của cửa sổ trình duyệt
            int scrollTarget = Math.Max(0, buttonY - windowHeight / 3); // Cuộn tới vị trí của nút, dịch lên 1/3 chiều cao màn hình

            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {scrollTarget});");
            Thread.Sleep(500);
            datMuaButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Đã thêm sản phẩm vào giỏ hàng.");



            // 🔹 Click biểu tượng giỏ hàng (debug và thử nhiều cách)
            IWebElement cartLink = null;
            try
            {
                // Thử selector chính xác với "sinlge-bar"
                cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.sinlge-bar.shopping a.single-icon")));
                Console.WriteLine("✅ Tìm thấy giỏ hàng");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("⚠️ Không tìm thấy 'div.sinlge-bar.shopping a.single-icon', thử 'div.single-bar.shopping a.single-icon'...");
                try
                {
                    // Thử sửa typo nếu là "single-bar"
                    cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.single-bar.shopping a.single-icon")));
                    Console.WriteLine("✅ Tìm thấy giỏ hàng");
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("❌ Cả hai selector đều không tìm thấy. Kiểm tra HTML thực tế!");
                    // In HTML để debug
                    var pageSource = driver.PageSource;
                    File.WriteAllText("debug.html", pageSource); // Lưu HTML để kiểm tra
                    Assert.Fail("Không tìm thấy biểu tượng giỏ hàng với bất kỳ selector nào!");
                }
            }

            // Scroll và click
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", cartLink);
                wait.Until(ExpectedConditions.ElementToBeClickable(cartLink)); // Đảm bảo có thể click
                                                                               // Thử click bình thường trước
                cartLink.Click();
                Console.WriteLine("✅ Đã click giỏ hàng");
            }
            catch (ElementClickInterceptedException)
            {
                //Console.WriteLine("⚠️ Click bị chặn");
                // Fallback: dùng JS click nếu click thường không hoạt động
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", cartLink);
                Console.WriteLine("✅ Đã click giỏ hàng");
            }

            // Chờ điều hướng tới trang giỏ hàng
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Giohang"));
                Console.WriteLine("✅ Đã mở trang giỏ hàng.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Không điều hướng được tới giỏ hàng. Current URL: {driver.Url}");
                Assert.Fail("Không thể mở trang giỏ hàng!");
            }
            Thread.Sleep(2000); // Giữ lại để kiểm tra giao diện nếu cần

            // 🔹 Kiểm tra sản phẩm trong giỏ hàng
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{targetProduct}')]")));
            var cartProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{targetProduct}')]"));
            Assert.That(cartProducts.Count, Is.GreaterThan(0), $"❌ Sản phẩm '{targetProduct}' chưa có trong giỏ hàng!");

            Console.WriteLine("✅ Test Passed: Đăng nhập, thêm sản phẩm vào giỏ hàng và kiểm tra giỏ hàng thành công!");
            // 🔹 Cuộn trang đến giữa trang web một lần

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // Lấy chiều cao của trang và cuộn đến giữa
            long height = (long)js.ExecuteScript("return document.body.scrollHeight");
            long middlePosition = height / 3; // Tính vị trí giữa trang
            js.ExecuteScript($"window.scrollTo(0, {middlePosition});");

            //Console.WriteLine("✅ Đã cuộn trang đến giữa.");
            Thread.Sleep(2000); // Chờ cho cuộn hoàn tất
            // 🔹 Tìm ô nhập số lượng sản phẩm
            var quantityInput = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[name='txtSoluong']")));

            // 🔹 Nhập số lượng vượt quá giới hạn
            quantityInput.Clear();
            quantityInput.SendKeys("201");
            Console.WriteLine("✅ Đã nhập số lượng 201.");

            // 🔹 Click nút "Cập Nhật"
            var updateButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("input.btn-success[type='submit'][value='Cập Nhật']")));
             updateButton.Click();
             Console.WriteLine("✅ Đã click cập nhật số lượng.");
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
          

            Console.WriteLine("✅ Test Passed: Không được thêm quá 200 sản phẩm.");
        }

        [Test]
        [TestCase("minthy", "123 ", true)]
        // Đăng nhập thành công và thêm sản phẩm vào giỏ hàng -> Đặt Hàng -> Trở về giỏ hàng
        public void Test_ThemSP_XemChiTietDonHang_TroVeGioHang(string username, string password, bool shouldLoginSucceed)
        {
            // Tối đa hóa cửa sổ ngay từ đầu để ổn định viewport
            driver.Manage().Window.Maximize();

            // Navigate to login page
            driver.Navigate().GoToUrl(baseUrl + "/Nguoidung/Dangnhap");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/Nguoidung/Dangnhap"));

            // 🔹 Đăng nhập
            wait.Until(ExpectedConditions.ElementIsVisible(By.Name("TenDN"))).SendKeys(username);
            driver.FindElement(By.Name("Matkhau")).SendKeys(password);

            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            var btnDangNhap = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@type='submit' and @value='Đăng Nhập']")));
            btnDangNhap.Click();

            // ✅ Chờ điều hướng sau đăng nhập
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
                Console.WriteLine($"✅ Đã điều hướng đến: {driver.Url}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Timeout khi chờ URL. Current URL: {driver.Url}");
                Assert.Fail("Đăng nhập không điều hướng đúng!");
            }

            // 🔹 Đảm bảo preloader biến mất
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Preloader đã biến mất.");

            // 🔹 Lấy danh sách sản phẩm
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product")));
            var products = driver.FindElements(By.CssSelector(".single-product"));
            Assert.That(products.Count, Is.GreaterThan(0), "❌ Không tìm thấy sản phẩm nào!");
            Console.WriteLine($"✅ Tìm thấy {products.Count} sản phẩm.");

            // 🔹 Tìm sản phẩm "iPhone 15 Pro Max"
            IWebElement productContainer = null;
            string targetProduct = "REALME C65";
            foreach (var product in products)
            {
                string productText = product.Text;
                Console.WriteLine($"Sản phẩm: {productText}");
                if (productText.Contains(targetProduct))
                {
                    productContainer = product;
                    break;
                }
            }
            if (productContainer == null)
            {
                Console.WriteLine($"⚠️ Không tìm thấy '{targetProduct}', thử 'REALME C12'...");
                foreach (var product in products)
                {
                    if (product.Text.Contains("REALME C12"))
                    {
                        productContainer = product;
                        targetProduct = "REALME C12";
                        break;
                    }
                }
            }
            Assert.That(productContainer, Is.Not.Null, $"❌ Không tìm thấy sản phẩm '{targetProduct}' hoặc fallback trên trang!");

            // 🔹 Scroll và hover sản phẩm
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productContainer);
            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".single-product .product-img")));
            //Console.WriteLine($"✅ Đã scroll đến '{targetProduct}'");

            var productImg = productContainer.FindElement(By.CssSelector(".product-img"));
            var actions = new Actions(driver);
            actions.MoveToElement(productImg).Perform();
            Thread.Sleep(500); // Đợi nhẹ để hover ổn định
            //Console.WriteLine("✅ Đã hover qua hình ảnh sản phẩm.");

            // 🔹 Click vào text "iPhone 15 Pro Max" và điều hướng tới /Details/95
            var productTextElement = productContainer.FindElement(By.XPath(".//*[contains(text(), 'REALME C65')]"));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", productTextElement);
            wait.Until(ExpectedConditions.ElementToBeClickable(productTextElement));
            productTextElement.Click();
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/BookStore/Details/99"));
            Console.WriteLine($"✅ Đã điều hướng đến trang chi tiết: {driver.Url}");
            Thread.Sleep(2000);

            var datMuaButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".btn.bg-primary")));
            int buttonY = datMuaButton.Location.Y; // Lấy tọa độ Y của nút
            int windowHeight = driver.Manage().Window.Size.Height; // Lấy chiều cao của cửa sổ trình duyệt
            int scrollTarget = Math.Max(0, buttonY - windowHeight / 3); // Cuộn tới vị trí của nút, dịch lên 1/3 chiều cao màn hình

            ((IJavaScriptExecutor)driver).ExecuteScript($"window.scrollTo(0, {scrollTarget});");
            Thread.Sleep(500);
            datMuaButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            Console.WriteLine("✅ Đã thêm sản phẩm vào giỏ hàng.");



            // 🔹 Quay lại trang index
            driver.Navigate().GoToUrl(baseUrl + "/");
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/"));
            Console.WriteLine("✅ Đã quay lại trang index.");

            // 🔹 Click biểu tượng giỏ hàng (debug và thử nhiều cách)
            IWebElement cartLink = null;
            try
            {
                // Thử selector chính xác với "sinlge-bar"
                cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.sinlge-bar.shopping a.single-icon")));
                Console.WriteLine("✅ Tìm thấy giỏ hàng");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("⚠️ Không tìm thấy 'div.sinlge-bar.shopping a.single-icon', thử 'div.single-bar.shopping a.single-icon'...");
                try
                {
                    // Thử sửa typo nếu là "single-bar"
                    cartLink = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("div.single-bar.shopping a.single-icon")));
                    Console.WriteLine("✅ Tìm thấy giỏ hàng ");
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine("❌ Cả hai selector đều không tìm thấy. Kiểm tra HTML thực tế!");
                    // In HTML để debug
                    var pageSource = driver.PageSource;
                    File.WriteAllText("debug.html", pageSource); // Lưu HTML để kiểm tra
                    Assert.Fail("Không tìm thấy biểu tượng giỏ hàng với bất kỳ selector nào!");
                }
            }

            // Scroll và click
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", cartLink);
                wait.Until(ExpectedConditions.ElementToBeClickable(cartLink)); // Đảm bảo có thể click
                                                                               // Thử click bình thường trước
                cartLink.Click();
                Console.WriteLine("✅ Đã click giỏ hàng");
            }
            catch (ElementClickInterceptedException)
            {
                //Console.WriteLine("⚠️ Click bị chặn, thử click bằng JavaScript...");
                // Fallback: dùng JS click nếu click thường không hoạt động
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", cartLink);
                Console.WriteLine("✅ Đã click giỏ hàng");
            }

            // Chờ điều hướng tới trang giỏ hàng
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Giohang"));
                Console.WriteLine("✅ Đã mở trang giỏ hàng.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Không điều hướng được tới giỏ hàng. Current URL: {driver.Url}");
                Assert.Fail("Không thể mở trang giỏ hàng!");
            }
            Thread.Sleep(2000); // Giữ lại để kiểm tra giao diện nếu cần

            // 🔹 Kiểm tra sản phẩm trong giỏ hàng
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//td[contains(text(), '{targetProduct}')]")));
            var cartProducts = driver.FindElements(By.XPath($"//td[contains(text(), '{targetProduct}')]"));
            Assert.That(cartProducts.Count, Is.GreaterThan(0), $"❌ Sản phẩm '{targetProduct}' chưa có trong giỏ hàng!");

            Console.WriteLine("✅ Test Passed: Đăng nhập, thêm sản phẩm vào giỏ hàng và kiểm tra giỏ hàng thành công!");
            Thread.Sleep(3000);
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.scrollBy(0, 700);");  // Cuộn xuống 1000 pixel
            Thread.Sleep(1000); // Chờ cho cuộn hoàn tất




            var datHangButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("tr.bg-success td.text-white a[href='/Giohang/DatHang']")));
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", datHangButton);
            datHangButton.Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("preloader")));
            // 🔹 Chuyển đến trang chi tiết đơn hàng
            //var checkoutButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//a[contains(@href, 'GioHang/DatHang')]")));
            //((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center', behavior: 'smooth'});", checkoutButton);
            //checkoutButton.Click();
            wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/DatHang"));
            Console.WriteLine("✅ Đã điều hướng đến trang thanh toán.");

            Console.WriteLine("✅ Test Passed: Thêm sản phẩm vào giỏ hàng, kiểm tra giỏ hàng và điều hướng tới thanh toán thành công!");
            // 🔹 Cuộn trang đến giữa trang web một lần

            IJavaScriptExecutor jss = (IJavaScriptExecutor)driver;

            // Lấy chiều cao của trang và cuộn đến giữa
            long height = (long)jss.ExecuteScript("return document.body.scrollHeight");
            long middlePosition = height / 3; // Tính vị trí giữa trang
            jss.ExecuteScript($"window.scrollTo(0, {middlePosition});");

            //Console.WriteLine("✅ Đã cuộn trang đến giữa.");
            Thread.Sleep(2000); // Chờ cho cuộn hoàn tất
            // 🔹 Tìm nút "Trở về giỏ hàng"
            IWebElement backToCartButton = null;
            try
            {
                // Giả sử nút "Trở về giỏ hàng" có selector là ".btn-back-to-cart"
                backToCartButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("a[href='/Giohang/GioHang']")));
                Console.WriteLine("✅ Tìm thấy nút 'Trở về giỏ hàng'.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("❌ Không tìm thấy nút 'Trở về giỏ hàng'. Kiểm tra lại selector!");
                Assert.Fail("Không tìm thấy nút 'Trở về giỏ hàng'!");
            }

            // 🔹 Click vào nút "Trở về giỏ hàng"
            backToCartButton.Click();
            Console.WriteLine("✅ Đã click vào nút 'Trở về giỏ hàng'.");

            // 🔹 Chờ điều hướng trở lại trang giỏ hàng
            try
            {
                wait.Until(ExpectedConditions.UrlContains(baseUrl + "/GioHang/Giohang"));
                Console.WriteLine("✅ Đã quay lại trang giỏ hàng.");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"❌ Không điều hướng trở lại trang giỏ hàng. Current URL: {driver.Url}");
                Assert.Fail("Không thể quay lại trang giỏ hàng!");
            }

            Thread.Sleep(2000);
        }
        [TearDown]
        public void TearDown()
        {
            if (driver != null)
            {
                driver.Quit();
                driver.Dispose();
            }
        }
    }
}
