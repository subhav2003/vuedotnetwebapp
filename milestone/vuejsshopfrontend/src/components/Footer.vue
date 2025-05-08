<template>
  <footer class="bg-[#23110d] py-8 text-[#C8B280]">
    <div class="container mx-auto text-center">
      <!-- Branding -->
      <p>&copy; 2024 Pustakalaya — All Rights Reserved</p>

      <!-- Social Links -->
      <div class="mt-4">
        <p class="text-[#C8B280]">Follow us on:</p>
        <div class="flex justify-center space-x-6 mt-2 text-sm">
          <a href="#" aria-label="Facebook" class="flex items-center hover:text-[#FFD700] transition duration-300">
            <i class="fab fa-facebook-f mr-2"></i> Facebook
          </a>
          <a href="#" aria-label="Instagram" class="flex items-center hover:text-[#FFD700] transition duration-300">
            <i class="fab fa-instagram mr-2"></i> Instagram
          </a>
          <a href="#" aria-label="Twitter" class="flex items-center hover:text-[#FFD700] transition duration-300">
            <i class="fab fa-twitter mr-2"></i> Twitter
          </a>
        </div>
      </div>

      <!-- Additional Links -->
      <div class="mt-6 space-x-2 text-sm">
        <a href="/privacy-policy" class="hover:text-[#FFD700] transition duration-300">Privacy Policy</a>
        <span class="text-gray-600">|</span>
        <a href="/terms" class="hover:text-[#FFD700] transition duration-300">Terms of Service</a>
        <span class="text-gray-600">|</span>
        <a href="/contact" class="hover:text-[#FFD700] transition duration-300">Contact Us</a>
      </div>
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
