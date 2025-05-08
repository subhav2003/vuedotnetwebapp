<template>
  <div class="flex flex-col min-h-screen bg-white">
    <!-- Fixed Top Navbar -->
    <header class="fixed top-0 left-0 w-full bg-dark-blue text-white z-50">
      <ArtisanNavBar />
    </header>

    <!-- Main Content (offset by navbar height) -->
    <main class="flex-1 pt-16 p-6 overflow-y-auto">
      <!-- Orders Section -->
      <section class="mb-8">

        <OrderTable />
      </section>

      <!-- Custom Orders Section -->
      <section>

        <CustomOrdersList />
      </section>
    </main>
  </div>
</template>

<script setup>
import ArtisanNavBar from "@/components/ArtisansDashboard/ArtisanNavBar.vue";
import OrderTable from "@/components/ArtisansDashboard/OrderTable.vue";
import CustomOrdersList from "@/components/ArtisansDashboard/CustomOrderList.vue";
</script>

<style scoped>
.bg-dark-blue {
  background-color: #2c3e50; /* Dark blue for the navbar */
}
</style>
