<template>
  <footer class="bg-[#0a0a0a] py-12">
    <div class="max-w-6xl mx-auto px-4 grid grid-cols-1 md:grid-cols-3 gap-8">
      <!-- About Section -->
      <div class="footer-about">
        <h3 class="text-xl text-white mb-4">About Pustakalaya</h3>
        <p class="text-gray-400">Pustakalaya is your go-to online bookstore for quality books across various genres.</p>
      </div>
      <!-- Quick Links -->
      <div class="footer-links">
        <h3 class="text-xl text-white mb-4">Quick Links</h3>
        <ul class="space-y-2">
          <li><router-link to="/about" class="hover:text-[#b25e31]">About Us</router-link></li>
          <li><router-link to="/contact" class="hover:text-[#b25e31]">Contact Us</router-link></li>
          <li><router-link to="/terms" class="hover:text-[#b25e31]">Terms of Service</router-link></li>
          <li><router-link to="/privacy" class="hover:text-[#b25e31]">Privacy Policy</router-link></li>
        </ul>
      </div>
      <!-- Social Links -->
      <div class="footer-social">
        <h3 class="text-xl text-white mb-4">Follow us on:</h3>
        <div class="flex space-x-4">
          <a href="#" target="_blank" rel="noopener noreferrer" class="w-9 h-9 bg-gray-800 rounded-full flex items-center justify-center hover:bg-[#b25e31] transition">
            <i class="fab fa-facebook text-gray-400 hover:text-white"></i>
          </a>
          <a href="#" target="_blank" rel="noopener noreferrer" class="w-9 h-9 bg-gray-800 rounded-full flex items-center justify-center hover:bg-[#b25e31] transition">
            <i class="fab fa-instagram text-gray-400 hover:text-white"></i>
          </a>
          <a href="#" target="_blank" rel="noopener noreferrer" class="w-9 h-9 bg-gray-800 rounded-full flex items-center justify-center hover:bg-[#b25e31] transition">
            <i class="fab fa-twitter text-gray-400 hover:text-white"></i>
          </a>
        </div>
      </div>
    </div>
    <div class="mt-8 border-t border-gray-700 pt-4 text-center text-gray-500">
      &copy; {{ new Date().getFullYear() }} Pustakalaya — All Rights Reserved
    </div>
  </footer>
</template>

<script>
export default {
  name: "FooterComponent",
};
</script>

<style scoped>
/* Ensure proper icon spacing */
i {
  margin-right: 0.5rem;
}
</style>